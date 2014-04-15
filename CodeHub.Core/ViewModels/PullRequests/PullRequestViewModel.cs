using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.Services;
using Cirrious.MvvmCross.Plugins.Messenger;
using CodeFramework.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : LoadableViewModel
    {
        private MvxSubscriptionToken _issueEditSubscription;
        private MvxSubscriptionToken _pullRequestEditSubscription;

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

        private bool _merged;

        public bool Merged
        {
            get { return _merged; }
            set
            {
                _merged = value;
                RaisePropertyChanged(() => Merged);
            }
        }

        private IssueModel _issueModel;

        public IssueModel Issue
        {
            get { return _issueModel; }
            set
            {
                _issueModel = value;
                RaisePropertyChanged(() => Issue);
            }
        }

        private PullRequestModel _model;

        public PullRequestModel PullRequest
        { 
            get { return _model; }
            set
            {
                _model = value;
                RaisePropertyChanged(() => PullRequest);
            }
        }

        private bool _isModifying;

        public bool IsModifying
        {
            get { return _isModifying; }
            set
            {
                _isModifying = value;
                RaisePropertyChanged(() => IsModifying);
            }
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
                    }, () => Issue != null);

                    this.Bind(x => Issue, cmd.RaiseCanExecuteChanged);
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
                    }, () => Issue != null);

                    this.Bind(x => Issue, cmd.RaiseCanExecuteChanged);
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
                    }, () => Issue != null);

                    this.Bind(x => Issue, cmd.RaiseCanExecuteChanged);
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
                }, () => Issue != null); 
            }
        }

        private ICommand _shareCommmand;

        public new ICommand ShareCommand
        {
            get
            { 
                if (_shareCommmand == null)
                {
                    var cmd = new MvxCommand(() => base.ShareCommand.Execute(PullRequest.HtmlUrl), () => PullRequest != null);
                    this.Bind(x => x.PullRequest, cmd.RaiseCanExecuteChanged);
                    _shareCommmand = cmd;
                }

                return _shareCommmand;
            }
        }

        public ICommand ToggleStateCommand
        {
            get { return new MvxCommand(() => ToggleState(PullRequest.State == "open")); }
        }

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

        protected override Task Load(bool forceCacheInvalidation)
        {
            var pullRequest = this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].Get();
            var t1 = this.RequestModel(pullRequest, forceCacheInvalidation, response => PullRequest = response.Data);
            Events.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetEvents(), forceCacheInvalidation).FireAndForget();
            Comments.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetComments(), forceCacheInvalidation).FireAndForget();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), forceCacheInvalidation, response => Issue = response.Data).FireAndForget();
            return t1;
        }

        public async Task Merge()
        {
            var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].Merge());
            if (!response.Data.Merged)
                throw new Exception(response.Data.Message);

            var pullRequest = this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].Get();
            await this.RequestModel(pullRequest, true, r => PullRequest = r.Data);
        }

        public ICommand MergeCommand
        {
            get { return new MvxCommand(() => Merge(), CanMerge); }
        }

        private bool CanMerge()
        {
            if (PullRequest == null)
                return false;
            return (PullRequest.Merged != null && PullRequest.Merged.Value == false && (PullRequest.Mergable == null || PullRequest.Mergable.Value));
        }

        public class NavObject
        {
            public string Username { get; set; }

            public string Repository { get; set; }

            public long Id { get; set; }
        }
    }
}
