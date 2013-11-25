using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp.Models;

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

        protected override Task Load(bool forceDataRefresh)
        {
			return Branches.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetBranches(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

