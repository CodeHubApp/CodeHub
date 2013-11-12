using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.User
{
    public class OrganizationMembersViewModel : BaseUserCollectionViewModel
    {
        public string OrganizationName 
        { 
            get; 
            private set; 
        }

        public void Init(NavObject navObject)
        {
            OrganizationName = navObject.Name;
        }

        public class NavObject
        {
            public string Name { get; set; }
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Users.SimpleCollectionLoad(Application.Client.Organizations[OrganizationName].GetMembers(), forceDataRefresh);
        }
    }
}

