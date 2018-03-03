using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.Messages;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.User;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Collections.Generic;

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

        public long Id
        { 
            get; 
            private set; 
        }

        public string Username
        { 
            get; 
            private set; 
        }

        public string Repository
        { 
            get; 
            private set; 
        }

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

        private IssueModel _issueModel;
        public IssueModel Issue
        {
            get { return _issueModel; }
            private set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        private PullRequestModel _model;
        public PullRequestModel PullRequest
        { 
            get { return _model; }
            private set { this.RaiseAndSetIfChanged(ref _model, value); }
        }

        private bool _isModifying;
        public bool IsModifying
        {
            get { return _isModifying; }
            set { this.RaiseAndSetIfChanged(ref _isModifying, value); }
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

        private ICommand _goToAssigneeCommand;

        public ICommand GoToAssigneeCommand
        {
            get
            {
                if (_goToAssigneeCommand == null)
                {
                    var cmd = new MvxCommand(() =>
                    {
                        GetService<IViewModelTxService>().Add(Issue.Assignee);
                        ShowViewModel<IssueAssignedToViewModel>(new IssueAssignedToViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                    }, () => Issue != null && IsCollaborator);

                    this.Bind(x => Issue).Subscribe(_ => cmd.RaiseCanExecuteChanged());
                    _goToAssigneeCommand = cmd;
                }

                return _goToAssigneeCommand;
            }
        }

        private ICommand _goToMilestoneCommand;

        public ICommand GoToMilestoneCommand
        {
            get
            { 
                if (_goToMilestoneCommand == null)
                {
                    var cmd = new MvxCommand(() =>
                    {
                        GetService<IViewModelTxService>().Add(Issue.Milestone);
                        ShowViewModel<IssueMilestonesViewModel>(new IssueMilestonesViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                    }, () => Issue != null && IsCollaborator);

                    this.Bind(x => Issue).Subscribe(_ => cmd.RaiseCanExecuteChanged());
                    _goToMilestoneCommand = cmd;
                }

                return _goToMilestoneCommand;
            }
        }

        private ICommand _goToLabelsCommand;

        public ICommand GoToLabelsCommand
        {
            get
            { 
                if (_goToLabelsCommand == null)
                {
                    var cmd = new MvxCommand(() =>
                    {
                        GetService<IViewModelTxService>().Add(Issue.Labels);
                        ShowViewModel<IssueLabelsViewModel>(new IssueLabelsViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                    }, () => Issue != null && IsCollaborator);

                    this.Bind(x => Issue).Subscribe(_ => cmd.RaiseCanExecuteChanged());
                    _goToLabelsCommand = cmd;
                }

                return _goToLabelsCommand;
            }
        }

        public ICommand GoToEditCommand
        {
            get
            { 
                return new MvxCommand(() =>
                {
                    GetService<IViewModelTxService>().Add(Issue);
                    ShowViewModel<IssueEditViewModel>(new IssueEditViewModel.NavObject { Username = Username, Repository = Repository, Id = Id });
                }, () => Issue != null && IsCollaborator); 
            }
        }

        public ICommand ToggleStateCommand
        {
            get { return new MvxCommand(() => ToggleState(PullRequest.State == "open")); }
        }

        public ReactiveUI.ReactiveCommand<Unit, bool> GoToOwner { get; }

        public ICommand GoToCommitsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<PullRequestCommitsViewModel>(new PullRequestCommitsViewModel.NavObject { Username = Username, Repository = Repository, PullRequestId = Id })); }
        }

        public ICommand GoToFilesCommand
        {
            get {
                return new MvxCommand(() =>
                {
                    ShowViewModel<PullRequestFilesViewModel>(new PullRequestFilesViewModel.NavObject {
                        Username = Username,
                        Repository = Repository,
                        PullRequestId = Id,
                        Sha = PullRequest.Head?.Sha
                    });   
                });
            }
        }

        public PullRequestViewModel(
            IApplicationService applicationService,
            IFeaturesService featuresService,
            IMessageService messageService,
            IMarkdownService markdownService)
        {
            _applicationService = applicationService;
            _featuresService = featuresService;
            _messageService = messageService;
            _markdownService = markdownService;

            this.Bind(x => x.PullRequest, true)
                .Where(x => x != null)
                .Select(x => string.Equals(x.State, "closed"))
                .Subscribe(x => IsClosed = x);

            this.Bind(x => x.Issue, true)
                .SelectMany(issue => _markdownService.Convert(issue?.Body).ToObservable())
                .Subscribe(x => MarkdownDescription = x);
            
            GoToOwner = ReactiveUI.ReactiveCommand.Create(
                () => ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = Issue?.User?.Login }),
                this.Bind(x => x.Issue, true).Select(x => x != null));
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;

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

        private async Task ToggleState(bool closed)
        {
            try
            {
                IsModifying = true;
                var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].UpdateState(closed ? "closed" : "open")); 
                _messageService.Send(new PullRequestEditMessage(data.Data));
            }
            catch (Exception e)
            {
                DisplayAlert("Unable to " + (closed ? "close" : "open") + " the item. " + e.Message);
            }
            finally
            {
                IsModifying = false;
            }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
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

            var pullRequest = this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].Get();
            var t1 = this.RequestModel(pullRequest, response => PullRequest = response.Data);
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), response => Issue = response.Data).ToBackground();

            _applicationService
                .GitHubClient.Repository.Get(Username, Repository)
                .ToBackground(x => 
                {
                    CanPush = x.Permissions.Push;
                    ShouldShowPro = x.Private && !_featuresService.IsProEnabled;
                });

            _applicationService
                .GitHubClient.Repository.Collaborator.IsCollaborator(Username, Repository, _applicationService.Account.Username)
                .ToBackground(x => IsCollaborator = x);
            
            return t1;
        }

        public async Task Merge()
        {
            try
            {
                var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].Merge(string.Empty));
                if (!response.Data.Merged)
                    throw new Exception(response.Data.Message);

            }
            catch (Exception e)
            {
                this.AlertService.Alert("Unable to Merge!", e.Message).ToBackground();
            }

            await Load();
        }

        public ICommand MergeCommand
        {
            get { return new MvxCommand(() => Merge(), CanMerge); }
        }

        private bool CanMerge()
        {
            if (PullRequest == null)
                return false;
            
            var isClosed = string.Equals(PullRequest.State, "closed", StringComparison.OrdinalIgnoreCase);
            var isMerged = PullRequest.Merged.GetValueOrDefault();

            return CanPush && !isClosed && !isMerged;
        }

        public class NavObject
        {
            public string Username { get; set; }

            public string Repository { get; set; }

            public long Id { get; set; }
        }
    }
}
