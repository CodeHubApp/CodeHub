using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.User
{
    public class TeamMembersViewModel : BaseUserCollectionViewModel
    {
        public ulong Id
        {
            get;
            private set;
        }

        public void Init(NavObject navObject)
        {
            Id = navObject.Id;
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Users.SimpleCollectionLoad(Application.Client.Teams[Id].GetMembers(), forceDataRefresh);
        }

        public class NavObject
        {
            public ulong Id { get; set; }
        }
    }
}