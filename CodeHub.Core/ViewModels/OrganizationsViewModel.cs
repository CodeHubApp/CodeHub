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

        public string OrganizationName 
        { 
            get; 
            private set; 
        }

        public void Init(NavObject navObject)
        {
            OrganizationName = navObject.Name;
        }


        public Task Load(bool forceDataRefresh)
        {
            return Organizations.SimpleCollectionLoad(Application.Client.Users[OrganizationName].GetOrganizations(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
	}
}

