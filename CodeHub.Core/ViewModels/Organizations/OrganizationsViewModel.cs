using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : LoadableViewModel
    {
        public CollectionViewModel<BasicUserModel> Organizations { get; }

        public string Username { get; private set; }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        public OrganizationsViewModel()
        {
            Title = "Organizations";
            Organizations = new CollectionViewModel<BasicUserModel>();
        }

        public ICommand GoToOrganizationCommand
        {
            get { return new MvxCommand<BasicUserModel>(x => ShowViewModel<OrganizationViewModel>(new OrganizationViewModel.NavObject { Name = x.Login }));}
        }

        protected override Task Load()
        {
            return Organizations.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].GetOrganizations());
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

