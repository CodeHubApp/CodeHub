using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.User
{
    public class UserFollowersViewModel : BaseUserCollectionViewModel
    {
        public string Name
        {
            get;
            private set;
        }

        public void Init(NavObject navObject)
        {
            Name = navObject.Username;
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Users.SimpleCollectionLoad(Application.Client.Users[Name].GetFollowers(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

