using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.User
{
    public class TeamMembersViewModel : BaseUserCollectionViewModel
    {
        public long Id { get; private set; }

        public void Init(NavObject navObject)
        {
            Id = navObject.Id;
        }

        protected override Task Load()
        {
            return Users.SimpleCollectionLoad(this.GetApplication().Client.Teams[Id].GetMembers());
        }

        public class NavObject
        {
            public long Id { get; set; }
        }
    }
}