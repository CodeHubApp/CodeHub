using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class ChangesetViewModel : LoadableViewModel
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

        public bool ShowRepository { get; private set; }

        public CommitModel Changeset
        {
            get { return _commitModel; }
            private set
            {
                _commitModel = value;
                RaisePropertyChanged(() => Changeset);
            }
        }

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = User, Repository = Repository })); }
        }

        public CollectionViewModel<CommentModel> Comments
        {
            get { return _comments; }
        }

        public ChangesetViewModel(IApplicationService application)
        {
            _application = application;
        }

        public void Init(NavObject navObject)
        {
            User = navObject.Username;
            Repository = navObject.Repository;
            Node = navObject.Node;
            ShowRepository = navObject.ShowRepository;
        }

        protected override Task Load(bool forceDataRefresh)
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
            public bool ShowRepository { get; set; }
        }
    }
}

