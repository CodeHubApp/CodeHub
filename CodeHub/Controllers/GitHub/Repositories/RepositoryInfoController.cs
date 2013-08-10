using CodeHub.Data;
using CodeHub.GitHub.Controllers.Branches;
using CodeHub.GitHub.Controllers.Changesets;
using CodeHub.GitHub.Controllers.Events;
using CodeHub.GitHub.Controllers.Followers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System;
using MonoTouch.Foundation;
using MonoTouch.Dialog.Utilities;
using CodeHub.GitHub.Controllers.Readme;
using CodeHub.Controllers;
using CodeHub.GitHub.Controllers.Accounts;
using CodeFramework.Controllers;
using CodeFramework.Views;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Repositories
{
    public class RepositoryInfoController : BaseModelDrivenController, IImageUpdated
    {
        private HeaderView _header;
        private readonly string _user;
        private readonly string _repo;

        public new RepositoryModel Model { get { return (RepositoryModel)base.Model; } }

        public RepositoryInfoController(string user, string repo)
            : base(typeof(RepositoryModel))
        {
            _user = user;
            _repo = repo;
            Title = repo;
            Root.UnevenRows = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _header = new HeaderView(View.Bounds.Width) { Title = _repo };
        }

        protected override void OnRender()
        {
            _header.Subtitle = "Updated " + Model.UpdatedAt.ToDaysAgo();
            _header.SetNeedsDisplay();

//            if (!string.IsNullOrEmpty(Model.Logo))
//                _header.Image = ImageLoader.DefaultRequestImage(new Uri(Model.Logo), this);

            Root.Add(new Section(_header));
            var sec1 = new Section();

            if (!string.IsNullOrEmpty(Model.Description) && !string.IsNullOrWhiteSpace(Model.Description))
            {
                var element = new MultilinedElement(Model.Description)
                {
                    BackgroundColor = UIColor.White
                };
                element.CaptionColor = element.ValueColor;
                element.CaptionFont = element.ValueFont;
                sec1.Add(element);
            }

            sec1.Add(new SplitElement(new SplitElement.Row
                                          {
                                              Text1 = "git",
                                              Image1 = Images.ScmType,
                                              Text2 = Model.Language,
                                              Image2 = Images.Language
                                          }));


            //Calculate the best representation of the size
            string size;
            if (Model.Size / 1024f < 1)
                size = string.Format("{0}KB", Model.Size);
            else if ((Model.Size / 1024f / 1024f) < 1)
                size = string.Format("{0:0.##}MB", Model.Size / 1024f);
            else
                size = string.Format("{0:0.##}GB", Model.Size / 1024f / 1024f);

            sec1.Add(new SplitElement(new SplitElement.Row
                                          {
                                              Text1 = Model.Private ? "Private" : "Public",
                                              Image1 = Model.Private ? Images.Locked : Images.Unlocked,
                                              Text2 = size,
                                              Image2 = Images.Size
                                          }));

            sec1.Add(new SplitElement(new SplitElement.Row
                                          {
                                              Text1 = Model.CreatedAt.ToString("MM/dd/yy"),
                                              Image1 = Images.Create,
                                              Text2 = Model.Forks.ToString() + (Model.Forks == 1 ? " Fork" : " Forks"),
                                              Image2 = Images.Fork
                                          }));


            var owner = new StyledStringElement("Owner", Model.Owner.Login) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            owner.Tapped += () => NavigationController.PushViewController(new ProfileController(Model.Owner.Login), true);
            sec1.Add(owner);
            var followers = new StyledStringElement("Watchers", "" + Model.Watchers) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            followers.Tapped += () => NavigationController.PushViewController(new RepoFollowersController(_user, _repo), true);
            sec1.Add(followers);


            var events = new StyledStringElement("Events", () => NavigationController.PushViewController(new RepoEventsController(_user, _repo), true), Images.Buttons.Event);

            var sec2 = new Section { events };

//            if (Model.HasIssues)
//                sec2.Add(new StyledStringElement("Issues", () => NavigationController.PushViewController(new IssuesController(_user, _repo), true), Images.Flag));

            if (Model.HasWiki)
                sec2.Add(new StyledStringElement("Wiki", () => NavigationController.PushViewController(new ReadmeController(_user, _repo), true), Images.Pencil));

            var sec3 = new Section
                           {
                new StyledStringElement("Changes", () => NavigationController.PushViewController(new ChangesetController(_user, _repo), true), Images.Changes),
                new StyledStringElement("Branches", () => NavigationController.PushViewController(new BranchController(_user, _repo), true), Images.Branch),
                new StyledStringElement("Tags", () => NavigationController.PushViewController(new TagController(_user, _repo), true), Images.Tag)
            };

            Root.Add(new[] { sec1, sec2, sec3 });

            if (!string.IsNullOrEmpty(Model.Homepage))
            {
                var web = new StyledStringElement("Website", () => UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(Model.Homepage)), Images.Webpage);
                Root.Add(new Section { web });
            }
        }

        protected override object OnUpdateModel(bool forced)
        {
            return Application.Client.API.GetRepository(_user, _repo).Data;
        }

        public void UpdatedImage(Uri uri)
        {
            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
            if (_header.Image != null)
                _header.SetNeedsDisplay();
        }

    }
}