using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.Services;
using MvvmCross.Plugins.Messenger;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Messages;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.User;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : LoadableViewModel
    {
        private MvxSubscriptionToken _issueEditSubscription;
        private MvxSubscriptionToken _pullRequestEditSubscription;
        private readonly IFeaturesService _featuresService;

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

        public string MarkdownDescription
        {
            get { return PullRequest == null ? string.Empty : (GetService<IMarkdownService>().Convert(PullRequest.Body)); }
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

        public ReactiveUI.ReactiveCommand<object> GoToOwner { get; }

        public ICommand GoToCommitsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<PullRequestCommitsViewModel>(new PullRequestCommitsViewModel.NavObject { Username = Username, Repository = Repository, PullRequestId = Id })); }
        }

        public ICommand GoToFilesCommand
        {
            get { return new MvxCommand(() => ShowViewModel<PullRequestFilesViewModel>(new PullRequestFilesViewModel.NavObject { Username = Username, Repository = Repository, PullRequestId = Id })); }
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

        public string ConvertToMarkdown(string str)
        {
            return (GetService<IMarkdownService>().Convert(str));
        }

        public PullRequestViewModel(IFeaturesService featuresService)
        {
            _featuresService = featuresService;

            this.Bind(x => x.PullRequest, true).IsNotNull().Select(x => string.Equals(x.State, "closed")).Subscribe(x => IsClosed = x);
            GoToOwner = ReactiveUI.ReactiveCommand.Create(this.Bind(x => x.Issue, true).Select(x => x != null));
            GoToOwner.Subscribe(_ => ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = Issue?.User?.Login }));
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;

            _issueEditSubscription = Messenger.SubscribeOnMainThread<IssueEditMessage>(x =>
            {
                if (x.Issue == null || x.Issue.Number != Id)
                    return;
                Issue = x.Issue;
            });

            _pullRequestEditSubscription = Messenger.SubscribeOnMainThread<PullRequestEditMessage>(x =>
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
                var comment = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].CreateComment(text));
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
                var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].UpdateState(closed ? "closed" : "open")); 
                Messenger.Publish(new PullRequestEditMessage(this) { PullRequest = data.Data });
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

            var pullRequest = this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].Get();
            var t1 = this.RequestModel(pullRequest, response => PullRequest = response.Data);
            Events.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetEvents()).FireAndForget();
            Comments.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetComments()).FireAndForget();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), response => Issue = response.Data).FireAndForget();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Get(), response => {
                CanPush = response.Data.Permissions.Push;
                ShouldShowPro = response.Data.Private && !_featuresService.IsProEnabled;
            }).FireAndForget();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].IsCollaborator(this.GetApplication().Account.Username), 
                response => IsCollaborator = response.Data).FireAndForget();
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

            await Load().FireAndForget();
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
