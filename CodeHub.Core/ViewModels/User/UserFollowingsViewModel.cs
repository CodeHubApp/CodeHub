using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.User
{
    public class UserFollowingsViewModel : BaseUserCollectionViewModel
    {
        public string Name { get; private set; }

        public void Init(NavObject navObject)
        {
            Name = navObject.Name;
        }

        protected override Task Load()
        {
            return Users.SimpleCollectionLoad(this.GetApplication().Client.Users[Name].GetFollowing());
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
    }
}