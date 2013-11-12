using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Source
{
    public class BranchesViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<BranchModel> _items = new CollectionViewModel<BranchModel>();
        private readonly IApplicationService _application;

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

        public ICommand GoToSourceCommand
        {
            get { return new MvxCommand<BranchModel>(x => ShowViewModel<SourceTreeViewModel>(new SourceTreeViewModel.NavObject { Username = Username, Repository = Repository, Branch = x.Name })); }
        }

        public BranchesViewModel(IApplicationService application)
        {
            _application = application;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Branches.SimpleCollectionLoad(_application.Client.Users[Username].Repositories[Repository].GetBranches(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

