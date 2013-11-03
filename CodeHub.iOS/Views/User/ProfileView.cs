using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;
using MonoTouch.Dialog.Utilities;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views
{
    [Register("ProfileView")]
    public class ProfileView : MvxTableViewController
    {
        private HeaderView _header;

        public override void ViewDidLoad()
        {
            _header = new HeaderView(View.Bounds.Width);

            base.ViewDidLoad();

            var set = this.CreateBindingSet<ProfileView, ProfileViewModel>();
            set.Bind(_header).To(x => x.Username).For(x => x.Title);
            set.Apply();

            //Root.Add(new Section(_header));

//            var followers = new StyledStringElement("Followers".t(), () => NavigationController.PushViewController(new UserFollowersViewController(username), true), Images.Heart);
//            var following = new StyledStringElement("Following".t(), () => NavigationController.PushViewController(new UserFollowingsViewController(username), true), Images.Following);
//            var events = new StyledStringElement("Events".t(), () => NavigationController.PushViewController(new UserEventsViewController(username), true), Images.Event);
//            var organizations = new StyledStringElement("Organizations".t(), () => NavigationController.PushViewController(new OrganizationsViewController(username), true), Images.Group);
//            var repos = new StyledStringElement("Repositories".t(), () => NavigationController.PushViewController(new UserRepositoriesViewController(username), true), Images.Repo);
//            var gists = new StyledStringElement("Gists", () => NavigationController.PushViewController(new AccountGistsViewController(username), true), Images.Script);

            //Root.Add(new [] { new Section { events, organizations, followers, following }, new Section { repos, gists } });
        }
//
//        public void UpdatedImage (System.Uri uri)
//        {
//            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
//            if (_header.Image != null)
//                _header.SetNeedsDisplay();
//        }
    }
}

