using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.User;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : LoadableViewModel
    {
        private IDisposable _editToken;
        private readonly IFeaturesService _featuresService;
        private readonly IApplicationService _applicationService;
        private readonly IMessageService _messageService;
        private readonly IMarkdownService _markdownService;

        public int Id { get; }

        public string Username { get; }

        public string Repository { get; }

        private string _markdownDescription;
        public string MarkdownDescription
        {
            get { return _markdownDescription; }
            private set { this.RaiseAndSetIfChanged(ref _markdownDescription, value); }
        }

        private bool? _isClosed;
        public bool? IsClosed
        {
            get { return _isClosed; }
            private set { this.RaiseAndSetIfChanged(ref _isClosed, value); }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        private bool _isCollaborator;
        public bool IsCollaborator
        {
            get { return _isCollaborator; }
            private set { this.RaiseAndSetIfChanged(ref _isCollaborator, value); }
        }

        private Octokit.Issue _issueModel;
        public Octokit.Issue Issue
        {
            get { return _issueModel; }
            private set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        private int? _participants;
        public int? Participants
        {
            get { return _participants; }
            set { this.RaiseAndSetIfChanged(ref _participants, value); }
        }

        private IReadOnlyList<Octokit.IssueComment> _comments;
        public IReadOnlyList<Octokit.IssueComment> Comments
        {
            get { return _comments ?? new List<Octokit.IssueComment>(); }
            private set { this.RaiseAndSetIfChanged(ref _comments, value); }
        }

        private IReadOnlyList<Octokit.EventInfo> _events;
        public IReadOnlyList<Octokit.EventInfo> Events
        {
            get { return _events ?? new List<Octokit.EventInfo>(); }
            private set { this.RaiseAndSetIfChanged(ref _events, value); }
        }

        public ReactiveCommand<Unit, UserViewModel> GoToOwner { get; }

        public ReactiveCommand<Unit, IssueEditViewModel> GoToEditCommand { get; }

        public ReactiveCommand<Unit, IssueLabelsViewModel> GoToLabelsCommand { get; }

        public ReactiveCommand<Unit, IssueMilestonesViewModel> GoToMilestoneCommand { get; }

        public ReactiveCommand<Unit, IssueAssignedToViewModel> GoToAssigneeCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleStateCommand { get; }

        protected override Task Load()
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
            {
                _applicationService
                    .GitHubClient.Repository.Get(Username, Repository)
                    .ToBackground(x => ShouldShowPro = x.Private && !_featuresService.IsProEnabled);
            }

            var issue = this.GetApplication().GitHubClient.Issue.Get(Username, Repository, Id)
                            .ContinueWith(r => Issue = r.Result, TaskContinuationOptions.OnlyOnRanToCompletion);

            _applicationService
                .GitHubClient.Issue.Comment.GetAllForIssue(Username, Repository, (int)Id)
                .ToBackground(x => Comments = x);

            _applicationService
                .GitHubClient.Issue.Events.GetAllForIssue(Username, Repository, (int)Id)
                .ToBackground(events =>
                {
                    Events = events;
                    Participants = events.Select(x => x.Actor?.Login).Distinct().Count();
                });

            _applicationService
                .GitHubClient.Repository.Collaborator.IsCollaborator(Username, Repository, _applicationService.Account.Username)
                .ToBackground(x => IsCollaborator = x);

            return issue;
        }

        public IssueViewModel(
            string username,
            string repository,
            int id,
            IApplicationService applicationService = null,
            IFeaturesService featuresService = null,
            IMessageService messageService = null,
            IMarkdownService markdownService = null)
        {
            Username = username;
            Repository = repository;
            Id = id;

            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _featuresService = featuresService ?? Locator.Current.GetService<IFeaturesService>();
            _messageService = messageService ?? Locator.Current.GetService<IMessageService>();
            _markdownService = markdownService ?? Locator.Current.GetService<IMarkdownService>();

            this.WhenAnyValue(x => x.Issue)
                .Where(x => x != null)
                .Select(x => x.State.Value == Octokit.ItemState.Closed)
                .Subscribe(x => IsClosed = x);

            this.WhenAnyValue(x => x.Issue)
                .SelectMany(issue => _markdownService.Convert(issue?.Body).ToObservable())
                .Subscribe(x => MarkdownDescription = x);

            GoToOwner = ReactiveCommand.Create(
                () => new UserViewModel(Issue.User.Login),
                this.WhenAnyValue(x => x.Issue.User.Login).Select(x => x != null));

            ToggleStateCommand = ReactiveCommand.CreateFromTask(ToggleState);

            ToggleStateCommand
                .ThrownExceptions
                .Select(err => new UserError("Failed to adjust pull request state!", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            var canEdit = this.WhenAnyValue(x => x.IsCollaborator).Select(x => x);

            GoToEditCommand = ReactiveCommand.Create<Unit, IssueEditViewModel>(
                _ => new IssueEditViewModel(username, repository, Issue),
                canEdit);

            GoToLabelsCommand = ReactiveCommand.Create<Unit, IssueLabelsViewModel>(
                _ => new IssueLabelsViewModel(username, repository, id, Issue.Labels),
                canEdit);

            GoToAssigneeCommand = ReactiveCommand.Create<Unit, IssueAssignedToViewModel>(
                _ => new IssueAssignedToViewModel(username, repository, id, Issue.Assignee),
                canEdit);

            GoToMilestoneCommand = ReactiveCommand.Create<Unit, IssueMilestonesViewModel>(
                _ => new IssueMilestonesViewModel(username, repository, id, Issue.Milestone),
                canEdit);

            _editToken = _messageService.Listen<IssueEditMessage>(x =>
            {
                if (x.Issue == null || x.Issue.Number != Issue.Number)
                    return;
                Issue = x.Issue;
            });
        }

        public async Task<bool> AddComment(string text)
        {
            try
            {
                var comment = await _applicationService.GitHubClient.Issue.Comment.Create(Username, Repository, (int)Id, text);
                var newCommentList = new List<Octokit.IssueComment>(Comments) { comment };
                Comments = newCommentList;
                return true;
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                return false;
            }
        }

        private async Task ToggleState()
        {
            var update = new Octokit.IssueUpdate
            {
                State = Issue.State.Value == Octokit.ItemState.Closed ? Octokit.ItemState.Open : Octokit.ItemState.Closed
            };

            var result = await this.GetApplication().GitHubClient.Issue.Update(Username, Repository, Id, update);
            _messageService.Send(new IssueEditMessage(result));
        }
    }
}

