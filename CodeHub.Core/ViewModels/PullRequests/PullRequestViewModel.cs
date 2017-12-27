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

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IApplicationService _applicationService;
        private IDisposable _issueEditSubscription;
        private IDisposable _pullRequestEditSubscription;
        private readonly IFeaturesService _featuresService;
        private readonly IMarkdownService _markdownService;

        public int Id
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

        private Octokit.Issue _issueModel;
        public Octokit.Issue Issue
        {
            get { return _issueModel; }
            private set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        private Octokit.PullRequest _model;
        public Octokit.PullRequest PullRequest
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

        private readonly CollectionViewModel<IssueCommentModel> _comments = new CollectionViewModel<IssueCommentModel>();

        public CollectionViewModel<IssueCommentModel> Comments
        {
            get { return _comments; }
        }

        private readonly CollectionViewModel<IssueEventModel> _events = new CollectionViewModel<IssueEventModel>();

        public CollectionViewModel<IssueEventModel> Events
        {
            get { return _events; }
        }

        public PullRequestViewModel(
            IFeaturesService featuresService,
            IMessageService messageService,
            IMarkdownService markdownService,
            IApplicationService applicationService)
        {
            _featuresService = featuresService;
            _messageService = messageService;
            _markdownService = markdownService;
            _applicationService = applicationService;

            this.Bind(x => x.PullRequest, true)
                .Where(x => x != null)
                .Subscribe(x => IsClosed = x.State.Value == Octokit.ItemState.Closed);

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
                var comment = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[Username].Repositories[Repository].Issues[Id].CreateComment(text));
                Comments.Items.Add(comment.Data);
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
                var state = closed ? Octokit.ItemState.Closed : Octokit.ItemState.Open;
                var pullRequestUpdate = new Octokit.PullRequestUpdate { State = state };
                var pullRequest = await _applicationService.GitHubClient.PullRequest.Update(Username, Repository, Id, pullRequestUpdate);
                _messageService.Send(new PullRequestEditMessage(pullRequest));
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

        protected override async Task Load()
        {
            ShouldShowPro = false;

            var pullRequest = _applicationService.GitHubClient.PullRequest.Get(Username, Repository, Id);
            var issue = _applicationService.GitHubClient.Issue.Get(Username, Repository, Id);

            Events.SimpleCollectionLoad(_applicationService.Client.Users[Username].Repositories[Repository].Issues[Id].GetEvents()).ToBackground();
            Comments.SimpleCollectionLoad(_applicationService.Client.Users[Username].Repositories[Repository].Issues[Id].GetComments()).ToBackground();

            this.RequestModel(_applicationService.Client.Users[Username].Repositories[Repository].Get(), response => {
                CanPush = response.Data.Permissions.Push;
                ShouldShowPro = response.Data.Private && !_featuresService.IsProEnabled;
            }).ToBackground();

            this.RequestModel(_applicationService.Client.Users[Username].Repositories[Repository].IsCollaborator(_applicationService.Account.Username), 
                response => IsCollaborator = response.Data).ToBackground();


            PullRequest = await pullRequest;
            Issue = await issue;
        }

        public async Task Merge()
        {
            try
            {
                var response = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[Username].Repositories[Repository].PullRequests[Id].Merge(string.Empty));
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

            var isClosed = PullRequest.State.Value == Octokit.ItemState.Closed;
            var isMerged = PullRequest.Merged;
            return CanPush && !isClosed && !isMerged;
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public int Id { get; set; }
        }
    }
}
