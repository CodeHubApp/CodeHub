using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels
{
    public class ChangesetViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<CommitModel> _commits = new CollectionViewModel<CommitModel>();

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

        public string Branch
        {
            get;
            private set;
        }

        public CollectionViewModel<CommitModel> Commits
        {
            get { return _commits; }
        }
        
        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Branch = navObject.Branch;
        }

        public Task Load(bool forceDataRefresh)
        {
            return Commits.SimpleCollectionLoad(GetRequest(), forceDataRefresh);
        }

        protected virtual GitHubRequest<List<CommitModel>> GetRequest()
        {
            return Application.Client.Users[Username].Repositories[Repository].Commits.GetAll(Branch);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public string Branch { get; set; }
        }
    }
}

