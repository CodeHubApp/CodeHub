using CodeHub.GitHub.Controllers.Followers;
using CodeHub.GitHub.Controllers.Repositories;
using MonoTouch.Dialog;
using GitHubSharp.Models;
using MonoTouch.Dialog.Utilities;
using CodeHub.GitHub.Controllers.Gists;
using CodeHub.Controllers;
using CodeHub.GitHub.Controllers.Organizations;
using CodeFramework.Controllers;
using CodeFramework.Views;
using CodeFramework.Elements;
using CodeHub.GitHub.Controllers.Events;

namespace CodeHub.GitHub.Controllers.Accounts
{
    public class ProfileController : BaseModelDrivenController, IImageUpdated
	{
        private HeaderView _header;

        public string Username { get; private set; }

        public new UserModel Model { get { return (UserModel)base.Model; } }

		public ProfileController(string username, bool push = true) 
            : base(typeof(UserModel))
		{
            Title = username;
			Username = username;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _header = new HeaderView(View.Bounds.Width) { Title = Username };
            Root.Add(new Section(_header));

            var followers = new StyledStringElement("Followers", () => NavigationController.PushViewController(new UserFollowersController(Username), true), Images.Heart);
            var events = new StyledStringElement("Events", () => NavigationController.PushViewController(new EventsController(Username) { ReportRepository = true }, true), Images.Buttons.Event);
            var groups = new StyledStringElement("Organizations", () => NavigationController.PushViewController(new OrganizationsController(Username), true), Images.Buttons.Group);
            var repos = new StyledStringElement("Repositories", () => NavigationController.PushViewController(new RepositoryController(Username) { ShowOwner = !Application.Accounts.ActiveAccount.Username.Equals(Username) }, true), Images.Repo);
            var gists = new StyledStringElement("Gists", () => NavigationController.PushViewController(new AccountGistsController(Username), true), Images.Script);

            Root.Add(new [] { new Section { followers, events, groups }, new Section { repos, gists} });
        }

        protected override void OnRender()
        {
            _header.Subtitle = Model.Name;
            _header.Image = ImageLoader.DefaultRequestImage(new System.Uri(Model.AvatarUrl), this);
            _header.SetNeedsDisplay();
        }

        protected override object OnUpdateModel(bool forced)
        {
            return Application.Client.API.GetUser(Username).Data;
        }

        public void UpdatedImage (System.Uri uri)
        {
            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
            if (_header.Image != null)
                _header.SetNeedsDisplay();
        }
	}
}


