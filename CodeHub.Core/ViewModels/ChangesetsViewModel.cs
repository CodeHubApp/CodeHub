using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels
{
    public class ChangesetsViewModel : LoadableViewModel
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

        public ICommand GoToChangesetCommand
        {
			get { return new MvxCommand<CommitModel>(x => ShowViewModel<ChangesetViewModel>(new ChangesetViewModel.NavObject { Username = Username, Repository = Repository, Node = x.Sha })); }
        }

        public CollectionViewModel<CommitModel> Commits
        {
            get { return _commits; }
        }
        
        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Branch = navObject.Branch ?? "master";
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Commits.SimpleCollectionLoad(GetRequest(), forceDataRefresh);
        }

        protected virtual GitHubRequest<List<CommitModel>> GetRequest()
        {
			return this.GetApplication().Client.Users[Username].Repositories[Repository].Commits.GetAll(Branch);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public string Branch { get; set; }
        }
    }
}

