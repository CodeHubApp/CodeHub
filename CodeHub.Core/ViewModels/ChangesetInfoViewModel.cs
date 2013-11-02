using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class ChangesetInfoViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<CommentModel> _comments = new CollectionViewModel<CommentModel>();
        private readonly IApplicationService _application;
        private CommitModel _commitModel;

        public string Node 
        { 
            get; 
            private set;
        }

        public string User 
        { 
            get; 
            private set; 
        }

        public string Repository 
        { 
            get; 
            private set; 
        }

        public CommitModel Changeset
        {
            get { return _commitModel; }
            private set
            {
                _commitModel = value;
                RaisePropertyChanged(() => Changeset);
            }
        }

        public CollectionViewModel<CommentModel> Comments
        {
            get { return _comments; }
        }

        public ChangesetInfoViewModel(IApplicationService application)
        {
            _application = application;
        }

        public void Init(NavObject navObject)
        {
            User = navObject.Username;
            Repository = navObject.Repository;
            Node = navObject.Node;
        }

        public Task Load(bool forceDataRefresh)
        {
            var t1 = Task.Run(() => this.RequestModel(_application.Client.Users[User].Repositories[Repository].Commits[Node].Get(), forceDataRefresh, response => Changeset = response.Data));
            FireAndForgetTask.Start(() => Comments.SimpleCollectionLoad(_application.Client.Users[User].Repositories[Repository].Commits[Node].Comments.GetAll(), forceDataRefresh));
            return t1;
        }

        public async Task AddComment(string text)
        {
            var c = await _application.Client.ExecuteAsync(_application.Client.Users[User].Repositories[Repository].Commits[Node].Comments.Create(text));
            Comments.Items.Add(c.Data);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public string Node { get; set; }
        }
    }
}

