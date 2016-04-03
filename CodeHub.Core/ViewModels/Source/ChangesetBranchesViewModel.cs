using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Changesets;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class ChangesetBranchesViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<BranchModel> _items = new CollectionViewModel<BranchModel>();

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

        public CollectionViewModel<BranchModel> Branches
        {
            get { return _items; }
        }

        public ICommand GoToBranchCommand
        {
            get { return new MvxCommand<BranchModel>(x => ShowViewModel<ChangesetsViewModel>(new ChangesetsViewModel.NavObject { Username = Username, Repository = Repository, Branch = x.Name })); }
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
        }

        protected override Task Load()
        {
            return Branches.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetBranches());
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

