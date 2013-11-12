using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : LoadableViewModel
	{
        private readonly CollectionViewModel<BasicUserModel> _orgs = new CollectionViewModel<BasicUserModel>();

        public CollectionViewModel<BasicUserModel> Organizations
        {
            get { return _orgs; }
        }

        public string Username 
        { 
            get; 
            private set; 
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        public ICommand GoToOrganizationCommand
        {
            get { return new MvxCommand<BasicUserModel>(x => ShowViewModel<OrganizationViewModel>(new OrganizationViewModel.NavObject { Name = x.Login }));}
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Organizations.SimpleCollectionLoad(Application.Client.Users[Username].GetOrganizations(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
	}
}

