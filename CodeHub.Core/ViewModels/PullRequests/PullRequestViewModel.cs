using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.Messages;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Collections.Generic;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IMessageService _messageService;
        private IDisposable _issueEditSubscription;
        private IDisposable _pullRequestEditSubscription;
        private readonly IFeaturesService _featuresService;
        private readonly IMarkdownService _markdownService;

        public long Id { get; }

        public string Username { get; }

        public string Repository { get; }

        private string _markdownDescription;
        public string MarkdownDescription
        {
            get { return _markdownDescription; }
            private set { this.RaiseAndSetIfChanged(ref _markdownDescription, value); }
        }

        private bool _canPush;
        public bool CanPush
        {
            get { return _canPush; }
            private set { this.RaiseAndSetIfChanged(ref _canPush, value); }
        }

        private ObservableAsPropertyHelper<bool> _canEdit;
        public bool CanEdit => _canEdit.Value;

        private bool _isCollaborator;
        public bool IsCollaborator
        {
            get { return _isCollaborator; }
            private set { this.RaiseAndSetIfChanged(ref _isCollaborator, value); }
        }

        private bool _merged;
        public bool Merged
        {
            get { return _merged; }
            set { this.RaiseAndSetIfChanged(ref _merged, value); }
        }

        private Octokit.Issue _issue;
        public Octokit.Issue Issue
        {
            get { return _issue; }
            private set { this.RaiseAndSetIfChanged(ref _issue, value); }
        }

        private Octokit.PullRequest _model;
        public Octokit.PullRequest PullRequest
        { 
            get { return _model; }
            private set { this.RaiseAndSetIfChanged(ref _model, value); }
        }

        private bool? _isClosed;
        public bool? IsClosed
        {
            get { return _isClosed; }
            private set { this.RaiseAndSetIfChanged(ref _isClosed, value); }
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

        public ReactiveCommand<Unit, IssueEditViewModel> GoToEditCommand { get; }

        public ReactiveCommand<Unit, IssueLabelsViewModel> GoToLabelsCommand { get; }

        public ReactiveCommand<Unit, IssueMilestonesViewModel> GoToMilestoneCommand { get; }

        public ReactiveCommand<Unit, IssueAssignedToViewModel> GoToAssigneeCommand { get; }

        public ReactiveCommand<Unit, string> GoToOwner { get; }

        public ReactiveCommand<Unit, Unit> MergeCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleStateCommand { get; }

        private bool _shouldShowPro;
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        public PullRequestViewModel(
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

            this.WhenAnyValue(x => x.PullRequest)
                .Where(x => x != null)
                .Select(x => x.State.Value == Octokit.ItemState.Closed)
                .Subscribe(x => IsClosed = x);

            this.WhenAnyValue(x => x.Issue)
                .SelectMany(issue => _markdownService.Convert(issue?.Body).ToObservable())
                .Subscribe(x => MarkdownDescription = x);
  
            MergeCommand = ReactiveCommand.CreateFromTask(Merge);

            MergeCommand
                .ThrownExceptions
                .Select(err => new UserError("Failed to merge!", "Unable to merge this pull request successfully!", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            ToggleStateCommand = ReactiveCommand.CreateFromTask(ToggleState);

            ToggleStateCommand
                .ThrownExceptions
                .Select(err => new UserError("Failed to adjust pull request state!", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            GoToOwner = ReactiveCommand.Create<Unit, string>(
                _ => Issue?.User?.Login ?? string.Empty,
                this.WhenAnyValue(x => x.Issue.User).Select(x => x != null));

            _canEdit = Observable.CombineLatest(
                this.WhenAnyValue(x => x.Issue).Select(x => x != null),
                this.WhenAnyValue(x => x.IsCollaborator).Select(x => x),
                (x, y) => x && y
            ).ToProperty(this, x => x.CanEdit);

            GoToEditCommand = ReactiveCommand.Create<Unit, IssueEditViewModel>(
                _ => new IssueEditViewModel(username, repository, Issue),
                this.WhenAnyValue(x => x.CanEdit));

            GoToLabelsCommand = ReactiveCommand.Create<Unit, IssueLabelsViewModel>(
                _ => new IssueLabelsViewModel(username, repository, id, Issue.Labels, true),
                this.WhenAnyValue(x => x.CanEdit));

            GoToAssigneeCommand = ReactiveCommand.Create<Unit, IssueAssignedToViewModel>(
                _ => new IssueAssignedToViewModel(username, repository, id, Issue.Assignee, true),
                this.WhenAnyValue(x => x.CanEdit));

            GoToMilestoneCommand = ReactiveCommand.Create<Unit, IssueMilestonesViewModel>(
                _ => new IssueMilestonesViewModel(username, repository, id, Issue.Milestone, true),
                this.WhenAnyValue(x => x.CanEdit));

            _issueEditSubscription = _messageService.Listen<IssueEditMessage>(x =>
            {
                if (x.Issue == null || x.Issue.Number != Id)
                    return;
                Issue = x.Issue;
            });

            _pullRequestEditSubscription = _messageService.Listen<PullRequestEditMessage>(x =>
            {
                if (x.PullRequest == null || x.PullRequest.Number != Id)
                    return;
                PullRequest = x.PullRequest;
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
            var update = new Octokit.PullRequestUpdate
            {
                State = (PullRequest.State == "open" ? Octokit.ItemState.Closed : Octokit.ItemState.Open)
            };

            var result = await this.GetApplication().GitHubClient.PullRequest.Update(Username, Repository, (int)Id, update);
            _messageService.Send(new PullRequestEditMessage(result));
        }

        protected override Task Load()
        {
            ShouldShowPro = false;

            _applicationService
                .GitHubClient.Issue.Comment.GetAllForIssue(Username, Repository, (int)Id)
                .ToBackground(x => Comments = x);

            _applicationService
                .GitHubClient.Issue.Events.GetAllForIssue(Username, Repository, (int)Id)
                .ToBackground(x => Events = x);

            var pullRequest = this.GetApplication().GitHubClient.PullRequest.Get(Username, Repository, (int)Id)
                .ContinueWith(r => PullRequest = r.Result, TaskContinuationOptions.OnlyOnRanToCompletion);

            var issue = this.GetApplication().GitHubClient.Issue.Get(Username, Repository, (int)Id)
                .ContinueWith(r => Issue = r.Result, TaskContinuationOptions.OnlyOnRanToCompletion);

            _applicationService
                .GitHubClient.Repository.Get(Username, Repository)
                .ToBackground(x => 
                {
                    CanPush = x.Permissions?.Push == true;
                    ShouldShowPro = x.Private && !_featuresService.IsProEnabled;
                });

            _applicationService
                .GitHubClient.Repository.Collaborator.IsCollaborator(Username, Repository, _applicationService.Account.Username)
                .ToBackground(x => IsCollaborator = x);
            
            return Task.WhenAll(pullRequest, issue);
        }

        public async Task Merge()
        {
            var merge = new Octokit.MergePullRequest
            {
                CommitMessage = string.Empty
            };

            var response = await this.GetApplication().GitHubClient.PullRequest.Merge(Username, Repository, (int)Id, merge);
            if (!response.Merged)
                throw new Exception(response.Message);

            await Load();
        }

        private bool CanMerge()
        {
            if (PullRequest == null)
                return false;

            var isClosed = PullRequest.State.Value == Octokit.ItemState.Closed;
            var isMerged = PullRequest.Merged;
            return CanPush && !isClosed && !isMerged;
        }
    }
}
