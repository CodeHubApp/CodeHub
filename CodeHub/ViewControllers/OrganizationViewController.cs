using CodeHub.Controllers;
using CodeFramework.Controllers;
using MonoTouch.Dialog.Utilities;
using GitHubSharp.Models;
using CodeFramework.Views;
using MonoTouch.Dialog;

namespace CodeHub.ViewControllers
{
    public class OrganizationViewController : BaseControllerDrivenViewController, IImageUpdated, IView<UserModel>
    {
        private HeaderView _header;
        public string Name { get; private set; }

        public new OrganizationController Controller
        {
            get { return (OrganizationController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public OrganizationViewController(string name)
        {
            Title = name;
            Name = name;
            Controller = new OrganizationController(this, name);
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
            _header = new HeaderView(View.Bounds.Width) { Title = Name };
            Root.Add(new Section(_header));

            var members = new StyledStringElement("Members".t(), () => NavigationController.PushViewController(new OrganizationMembersViewController(Name), true), Images.Buttons.Person);
            var teams = new StyledStringElement("Teams".t(), () => NavigationController.PushViewController(new TeamsViewController(Name), true), Images.Team);


            var followers = new StyledStringElement("Followers".t(), () => NavigationController.PushViewController(new UserFollowersViewController(Name), true), Images.Heart);
            var events = new StyledStringElement("Events".t(), () => NavigationController.PushViewController(new EventsViewController(Name), true), Images.Buttons.Event);
            var repos = new StyledStringElement("Repositories".t(), () => NavigationController.PushViewController(new RepositoriesViewController(Name), true), Images.Repo);
            var gists = new StyledStringElement("Gists", () => NavigationController.PushViewController(new AccountGistsViewController(Name), true), Images.Script);

            Root.Add(new [] { new Section { members, teams }, new Section { events, followers }, new Section { repos, gists } });
        }

        public void UpdatedImage (System.Uri uri)
        {
            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
            if (_header.Image != null)
                _header.SetNeedsDisplay();
        }
    }
}

