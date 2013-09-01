using CodeHub.Controllers;
using CodeFramework.Controllers;
using MonoTouch.Dialog.Utilities;
using GitHubSharp.Models;
using CodeFramework.Views;
using MonoTouch.Dialog;

namespace CodeHub.ViewControllers
{
    public class ProfileViewController : BaseControllerDrivenViewController, IImageUpdated, IView<UserModel>
    {
        private HeaderView _header;
        public string Username { get; private set; }

        public new ProfileController Controller
        {
            get { return (ProfileController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public ProfileViewController(string username)
        {
            Title = username;
            Username = username;
            Controller = new ProfileController(this, username);
        }

        public void Render(UserModel model)
        {
            _header.Subtitle = string.IsNullOrEmpty(model.Name) ? model.Login : model.Name;
            _header.Image = ImageLoader.DefaultRequestImage(new System.Uri(model.AvatarUrl), this);
            _header.SetNeedsDisplay();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _header = new HeaderView(View.Bounds.Width) { Title = Username };
            Root.Add(new Section(_header));

            var followers = new StyledStringElement("Followers".t(), () => NavigationController.PushViewController(new UserFollowersViewController(Username), true), Images.Heart);
            var following = new StyledStringElement("Following".t(), () => NavigationController.PushViewController(new UserFollowingsViewController(Username), true), Images.Heart);
            var events = new StyledStringElement("Events".t(), () => NavigationController.PushViewController(new EventsViewController(Username), true), Images.Buttons.Event);
            var organizations = new StyledStringElement("Organizations".t(), () => NavigationController.PushViewController(new OrganizationsViewController(Username), true), Images.Buttons.Group);
            var repos = new StyledStringElement("Repositories".t(), () => NavigationController.PushViewController(new RepositoriesViewController(Username), true), Images.Repo);
            var gists = new StyledStringElement("Gists", () => NavigationController.PushViewController(new AccountGistsViewController(Username), true), Images.Script);

            Root.Add(new [] { new Section { events, organizations, followers, following }, new Section { repos, gists } });
        }

        public void UpdatedImage (System.Uri uri)
        {
            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
            if (_header.Image != null)
                _header.SetNeedsDisplay();
        }
    }
}

