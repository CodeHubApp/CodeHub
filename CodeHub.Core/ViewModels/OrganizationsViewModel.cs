using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class OrganizationsViewModel : BaseViewModel, ILoadableViewModel
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


        public Task Load(bool forceDataRefresh)
        {
            return Organizations.SimpleCollectionLoad(Application.Client.Users[Username].GetOrganizations(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
	}
}

