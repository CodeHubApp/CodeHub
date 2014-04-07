using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Plugins.Messenger;
using CodeHub.Core.Messages;
using CodeFramework.Core.Services;
using CodeHub.Core.Services;
using System;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : LoadableViewModel
    {
		private MvxSubscriptionToken _editToken;

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

		public ICommand GoToAssigneeCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<IViewModelTxService>().Add(Issue.Assignee);
					ShowViewModel<IssueAssignedToViewModel>(new IssueAssignedToViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
				}); 
			}
		}

		public ICommand GoToMilestoneCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<IViewModelTxService>().Add(Issue.Milestone);
					ShowViewModel<IssueMilestonesViewModel>(new IssueMilestonesViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
				}); 
			}
		}

		public ICommand GoToLabelsCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<IViewModelTxService>().Add(Issue.Labels);
					ShowViewModel<IssueLabelsViewModel>(new IssueLabelsViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
				}); 
			}
		}

		public ICommand GoToEditCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<IViewModelTxService>().Add(Issue);
					ShowViewModel<IssueEditViewModel>(new IssueEditViewModel.NavObject { Username = Username, Repository = Repository, Id = Id });
				}, () => Issue != null); 
			}
		}

        public ICommand ToggleStateCommand
        {
            get 
            {
                return new MvxCommand(() => ToggleState(Issue.State == "open"), () => Issue != null);
            }
        }

        public ICommand ShareCommand
        {
            get
            {
                return new MvxCommand(() => GetService<IShareService>().ShareUrl(Issue.HtmlUrl), () => Issue != null & !string.IsNullOrEmpty(Issue.HtmlUrl));
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
			var t1 = this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), forceCacheInvalidation, response => Issue = response.Data);
            Comments.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetComments(), forceCacheInvalidation).FireAndForget();
            Events.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetEvents(), forceCacheInvalidation).FireAndForget();
            return t1;
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

			_editToken = Messenger.SubscribeOnMainThread<IssueEditMessage>(x =>
			{
				if (x.Issue == null || x.Issue.Number != Issue.Number)
					return;
				Issue = x.Issue;
			});
        }

        public async Task AddComment(string text)
        {
			var comment = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].CreateComment(text));
            Comments.Items.Add(comment.Data);
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

