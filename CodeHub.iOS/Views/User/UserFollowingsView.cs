using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels.User;

namespace CodeHub.iOS.Views.User
{
    public class UserFollowingsView : BaseFollowersView
    {
        public new UserFollowingsViewModel ViewModel
        {
            get { return (UserFollowingsViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            Title = "Following".t();
            SearchPlaceholder = "Search Following".t();
            NoItemsText = "Not Following Anyone".t();  

            BindCollection(ViewModel.Users, x =>
            {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Tapped += () => ViewModel.GoToUserCommand.Execute(x);
                return e;
            });

            base.ViewDidLoad();
        }
    }
}

