using System;
using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.ViewModels.User;
using Cirrious.MvvmCross.Plugins.Messenger;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : LoadableViewModel
    {
		private MvxSubscriptionToken _editToken;

        public ulong Id 
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

		public ICommand GoToAssigneeCommand
		{
			get { return new MvxCommand(() => ShowViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = Issue.Assignee.Login }), () => Issue != null && Issue.Assignee != null); }
		}

		public ICommand GoToEditCommand
		{
			get 
			{ 
				return new MvxCommand(() => {
					GetService<CodeFramework.Core.Services.IViewModelTxService>().Add(Issue);
					ShowViewModel<IssueEditViewModel>(new IssueEditViewModel.NavObject { Username = Username, Repository = Repository, Id = Id });
				}, () => Issue != null); 
			}
		}

		private readonly CollectionViewModel<IssueCommentModel> _comments = new CollectionViewModel<IssueCommentModel>();
        public CollectionViewModel<IssueCommentModel> Comments
        {
            get { return _comments; }
        }

        public Action<IssueModel> ModelChanged;

        protected override Task Load(bool forceCacheInvalidation)
        {
			var t1 = Task.Run(() => this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), forceCacheInvalidation, response => Issue = response.Data));

			FireAndForgetTask.Start(() => this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetComments(), forceCacheInvalidation, response => {
                Comments.Items.Reset(response.Data);
				this.CreateMore(response, m => Comments.MoreItems = m, Comments.Items.AddRange);
            }));

            return t1;
        }

        public string ConvertToMarkdown(string str)
        {
            var options = new MarkdownSharp.MarkdownOptions();
            options.AutoHyperlink = true;
            var md = new MarkdownSharp.Markdown(options);
            return md.Transform(str);
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

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public ulong Id { get; set; }
        }
    }
}

