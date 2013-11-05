using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.Views.User
{
    public class UserFollowersView : BaseFollowersView
    {
        public new UserFollowersViewModel ViewModel
        {
            get { return (UserFollowersViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            Title = "Followers".t();
            SearchPlaceholder = "Search Followers".t();
            NoItemsText = "No Followers".t();

            base.ViewDidLoad();

            BindCollection(ViewModel.Users, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Tapped += () => ViewModel.GoToUserCommand.Execute(x);
                return e;
            });
        }
    }
}

