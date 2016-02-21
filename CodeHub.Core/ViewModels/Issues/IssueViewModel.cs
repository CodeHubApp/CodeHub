using System.Threading.Tasks;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using System.Windows.Input;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using System;
using MvvmCross.Plugins.Messenger;
using MvvmCross.Core.ViewModels;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : LoadableViewModel
    {
		private MvxSubscriptionToken _editToken;
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
			get
			{
				if (Issue == null)
					return string.Empty;
				return (GetService<IMarkdownService>().Convert(Issue.Body));
			}
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

		private IssueModel _issueModel;
        public IssueModel Issue
        {
            get { return _issueModel; }
            private set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        private bool _isModifying;
        public bool IsModifying
        {
            get { return _isModifying; }
            set { this.RaiseAndSetIfChanged(ref _isModifying, value); }
        }

		public ICommand GoToAssigneeCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<IViewModelTxService>().Add(Issue.Assignee);
					ShowViewModel<IssueAssignedToViewModel>(new IssueAssignedToViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                }, () =>  IsCollaborator); 
			}
		}

		public ICommand GoToMilestoneCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<IViewModelTxService>().Add(Issue.Milestone);
					ShowViewModel<IssueMilestonesViewModel>(new IssueMilestonesViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                }, () =>  IsCollaborator); 
			}
		}

		public ICommand GoToLabelsCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<IViewModelTxService>().Add(Issue.Labels);
					ShowViewModel<IssueLabelsViewModel>(new IssueLabelsViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                }, () =>  IsCollaborator); 
			}
		}

		public ICommand GoToEditCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<IViewModelTxService>().Add(Issue);
					ShowViewModel<IssueEditViewModel>(new IssueEditViewModel.NavObject { Username = Username, Repository = Repository, Id = Id });
                }, () => Issue != null && IsCollaborator); 
			}
		}

        public ICommand ToggleStateCommand
        {
            get 
            {
                return new MvxCommand(() => ToggleState(Issue.State == "open"), () => Issue != null);
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

        protected override Task Load(bool forceCacheInvalidation)
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
                this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Get(), false, x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);

			var t1 = this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), forceCacheInvalidation, response => Issue = response.Data);
            Comments.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetComments(), forceCacheInvalidation).FireAndForget();
            Events.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetEvents(), forceCacheInvalidation).FireAndForget();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].IsCollaborator(this.GetApplication().Account.Username), 
                forceCacheInvalidation, response => IsCollaborator = response.Data).FireAndForget();
            return t1;
        }

        public string ConvertToMarkdown(string str)
        {
			return (GetService<IMarkdownService>().Convert(str));
        }

        public IssueViewModel(IFeaturesService featuresService)
        {
            _featuresService = featuresService;
            this.Bind(x => x.Issue, true).Where(x => x != null).Select(x => string.Equals(x.State, "closed")).Subscribe(x => IsClosed = x);
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;

			_editToken = Messenger.SubscribeOnMainThread<IssueEditMessage>(x =>
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
                var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Issue.Number].UpdateState(closed ? "closed" : "open")); 
                Messenger.Publish(new IssueEditMessage(this) { Issue = data.Data });
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

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public long Id { get; set; }
        }
    }
}

