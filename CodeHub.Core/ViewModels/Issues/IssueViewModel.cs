using System;
using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : LoadableViewModel
    {
        private IssueModel _issueModel;
        private readonly CollectionViewModel<IssueCommentModel> _comments = new CollectionViewModel<IssueCommentModel>();

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

        public IssueModel Issue
        {
            get { return _issueModel; }
            set
            {
                _issueModel = value;
                RaisePropertyChanged(() => Issue);
            }
        }

        public CollectionViewModel<IssueCommentModel> Comments
        {
            get { return _comments; }
        }

        public Action<IssueModel> ModelChanged;

        protected override Task Load(bool forceDataRefresh)
        {
            var t1 = Task.Run(() => this.RequestModel(Application.Client.Users[Username].Repositories[Repository].Issues[Id].Get(), forceDataRefresh, response => Issue = response.Data));

            FireAndForgetTask.Start(() => this.RequestModel(Application.Client.Users[Username].Repositories[Repository].Issues[Id].GetComments(), forceDataRefresh, response => {
                Comments.Items.Reset(response.Data);
                this.CreateMore(response, m => Comments.MoreItems = m, d => Comments.Items.AddRange(d));
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
        }

        public async Task AddComment(string text)
        {
            var comment = await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[Repository].Issues[Id].CreateComment(text));
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

