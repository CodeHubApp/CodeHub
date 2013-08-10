using CodeHub.Controllers;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.Dialog.Utilities;
using System;
using CodeHub.GitHub.Controllers.Followers;
using CodeHub.GitHub.Controllers.Gists;
using CodeHub.GitHub.Controllers.Repositories;
using MonoTouch.Foundation;
using CodeFramework.Controllers;
using CodeFramework.Views;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Organizations
{
    public class OrganizationInfoController : BaseModelDrivenController, IImageUpdated
    {
        private HeaderView _header;
        public string Org { get; private set; }

        public new UserModel Model { get { return (UserModel)base.Model; } }
        
        public OrganizationInfoController(string org)
            : base(typeof(UserModel))
        {
            Org = org;
            Title = org;
            Root.UnevenRows = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _header = new HeaderView(View.Bounds.Width) { Title = Org };
        }

        protected override void OnRender()
        {
            _header.Subtitle = Model.Company;
            if (!string.IsNullOrEmpty(Model.AvatarUrl))
                _header.Image = ImageLoader.DefaultRequestImage(new Uri(Model.AvatarUrl), this);
            _header.SetNeedsDisplay();

            var sec = new Section();
            var sec2 = new Section();
            var root = new RootElement(Title) { new Section(_header), sec, sec2 };

            var followers = new StyledStringElement("Followers", () => NavigationController.PushViewController(new UserFollowersController(Org), true), Images.Heart);
            sec.Add(followers);

            //var events = new StyledStringElement("Events", () => NavigationController.PushViewController(new EventsController(Username) { ReportRepository = true }, true), Images.Event);

            var gists = new StyledStringElement("Gists", () => NavigationController.PushViewController(new AccountGistsController(Org), true), Images.Repo );
            sec.Add(gists);

            var repos = new StyledStringElement("Repositories", () => NavigationController.PushViewController(new RepositoryController(Org), true), Images.Repo);
            sec.Add(repos);

            if (!String.IsNullOrEmpty(Model.Blog))
            {
                var blog = new StyledStringElement("Blog", () => UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(Model.Blog)), Images.Webpage);
                sec2.Add(blog);
            }

            Root = root;
        }

        protected override object OnUpdateModel(bool forced)
        {
            return Application.Client.API.GetOrganization(Org).Data;
        }

        public void UpdatedImage(Uri uri)
        {
            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
            if (_header.Image != null)
                _header.SetNeedsDisplay();
        }
    }
}
