using GitHubSharp.Models;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System;
using MonoTouch.Foundation;
using MonoTouch.Dialog.Utilities;
using CodeHub.Controllers;
using CodeFramework.Views;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using System.Threading.Tasks;

namespace CodeHub.ViewControllers
{
    public class RepositoryInfoViewController : BaseControllerDrivenViewController, IImageUpdated, IView<RepositoryInfoController.ViewModel>
    {
        private HeaderView _header;

        public string Username { get; private set; }

        public string Slug { get; private set; }

        public new RepositoryInfoController Controller
        {
            get { return (RepositoryInfoController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public RepositoryInfoViewController(string username, string slug,  string name)
        {
            Username = username;
            Slug = slug;
            Title = name;

            _header = new HeaderView(View.Bounds.Width) { Title = name, ShadowImage = false };

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.GearButton, ShowExtraMenu));
            NavigationItem.RightBarButtonItem.Enabled = false;

            Controller = new RepositoryInfoController(this, username, slug);
        }

        public RepositoryInfoViewController(string username, string slug)
            : this(username, slug, slug)
        {
        }

        private void ShowExtraMenu()
        {
            var repoModel = Controller.Model.RepositoryModel;
            var sheet = MonoTouch.Utilities.GetSheet(repoModel.Name);

            var pinButton = sheet.AddButton(Application.Account.GetPinnedRepository(repoModel.Owner.Login, repoModel.Name) == null ? "Pin to Slideout Menu".t() : "Unpin from Slideout Menu".t());
            var starButton = sheet.AddButton(Controller.Model.IsStarred ? "Unstar This Repo".t() : "Star This Repo".t());
            var watchButton = sheet.AddButton(Controller.Model.IsWatched ? "Unwatch This Repo".t() : "Watch This Repo".t());
            //var forkButton = sheet.AddButton("Fork Repository".t());
            var showButton = sheet.AddButton("Show in GitHub".t());
            var cancelButton = sheet.AddButton("Cancel".t());
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Clicked += (s, e) => {
                // Pin to menu
                if (e.ButtonIndex == pinButton)
                {
                    //Is it pinned already or not?
                    var pinnedRepo = Application.Account.GetPinnedRepository(repoModel.Owner.Login, repoModel.Name);
                    if (pinnedRepo == null)
                    {
                        var imageUrl = repoModel.Fork ? CodeHub.Images.GitHubRepoForkUrl : CodeHub.Images.GitHubRepoUrl;
                        Application.Account.AddPinnedRepository(repoModel.Owner.Login, repoModel.Name, repoModel.Name, imageUrl.AbsolutePath);
                    }
                    else
                        Application.Account.RemovePinnedRepository(pinnedRepo.Id);
                }
                // Watch this repo
                else if (e.ButtonIndex == starButton)
                {
                    this.DoWork(() => {
                        if (Controller.Model.IsStarred)
                            Controller.Unstar();
                        else
                            Controller.Star();
                    }, ex => {
                        MonoTouch.Utilities.ShowAlert("Error".t(), ex.Message);
                    });
                }
                // Watch this repo
                else if (e.ButtonIndex == watchButton)
                {
                    this.DoWork(() => {
                        if (Controller.Model.IsWatched)
                            Controller.StopWatching();
                        else
                            Controller.Watch();
                    }, ex => {
                        MonoTouch.Utilities.ShowAlert("Error".t(), ex.Message);
                    });
                }
                // Fork this repo
//                else if (e.ButtonIndex == forkButton)
//                {
//                    ForkRepository();
//                }
                // Show in Bitbucket
                else if (e.ButtonIndex == showButton)
                {
                    try { UIApplication.SharedApplication.OpenUrl(new NSUrl(repoModel.HtmlUrl)); } catch { }
                }
            };

            sheet.ShowInView(this.View);
        }

//        private void ForkRepository()
//        {
//            var repoModel = Controller.Model.RepositoryModel;
//            var alert = new UIAlertView();
//            alert.Title = "Fork".t();
//            alert.Message = "What would you like to name your fork?".t();
//            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
//            var forkButton = alert.AddButton("Fork!".t());
//            var cancelButton = alert.AddButton("Cancel".t());
//            alert.CancelButtonIndex = cancelButton;
//            alert.DismissWithClickedButtonIndex(cancelButton, true);
//            alert.GetTextField(0).Text = repoModel.Name;
//            alert.Clicked += (object sender2, UIButtonEventArgs e2) => {
//                if (e2.ButtonIndex == forkButton)
//                {
//                    var text = alert.GetTextField(0).Text;
//                    this.DoWork("Forking...".t(), () => {
//                        //var fork = Application.Client.Users[model.Owner.Login].Repositories[model.Name].Fo(text);
//                        BeginInvokeOnMainThread(() => {
//                            //  NavigationController.PushViewController(new RepositoryInfoViewController(fork), true);
//                        });
//                    }, (ex) => {
//                        //We typically get a 'BAD REQUEST' but that usually means that a repo with that name already exists
//                        MonoTouch.Utilities.ShowAlert("Unable to fork".t(), "A repository by that name may already exist in your collection or an internal error has occured.".t());
//                    });
//                }
//            };
//
//            alert.Show();
//        }

        public void Render(RepositoryInfoController.ViewModel viewModel)
        {
            var model = viewModel.RepositoryModel;
            Title = model.Name;
            var root = new RootElement(Title) { UnevenRows = true };
            _header.Subtitle = "Updated ".t() + (model.UpdatedAt).ToDaysAgo();
            var imageUrl = model.Fork ? CodeHub.Images.GitHubRepoForkUrl : CodeHub.Images.GitHubRepoUrl;
            _header.Image = ImageLoader.DefaultRequestImage(imageUrl, this);

            root.Add(new Section(_header));
            var sec1 = new Section();

            if (!string.IsNullOrEmpty(model.Description) && !string.IsNullOrWhiteSpace(model.Description))
            {
                var element = new MultilinedElement(model.Description)
                {
                    BackgroundColor = UIColor.White
                };
                element.CaptionColor = element.ValueColor;
                element.CaptionFont = element.ValueFont;
                sec1.Add(element);
            }

            sec1.Add(new SplitElement(new SplitElement.Row {
                Text1 = "git",
                Image1 = Images.ScmType,
                Text2 = model.Language,
                Image2 = Images.Language
            }));


            //Calculate the best representation of the size
            string size;
            if (model.Size / 1024f < 1)
                size = string.Format("{0:0.##}KB", model.Size);
            else if ((model.Size / 1024f / 1024f) < 1)
                size = string.Format("{0:0.##}MB", model.Size / 1024f);
            else
                size = string.Format("{0:0.##}GB", model.Size / 1024f / 1024f);

            sec1.Add(new SplitElement(new SplitElement.Row {
                Text1 = model.Private ? "Private".t() : "Public".t(),
                Image1 = model.Private ? Images.Locked : Images.Unlocked,
                Text2 = size,
                Image2 = Images.Size
            }));

            sec1.Add(new SplitElement(new SplitElement.Row {
                Text1 = (model.CreatedAt).ToString("MM/dd/yy"),
                Image1 = Images.Create,
                Text2 = model.Forks.ToString() + (model.Forks == 1 ? " Fork".t() : " Forks".t()),
                Image2 = Images.Fork
            }));


            var owner = new StyledStringElement("Owner".t(), model.Owner.Login) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            owner.Tapped += () => NavigationController.PushViewController(new ProfileViewController(model.Owner.Login), true);
            sec1.Add(owner);
            var followers = new StyledStringElement("Stargazers".t(), "" + model.Watchers) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            followers.Tapped += () => NavigationController.PushViewController(new RepoFollowersViewController(model.Owner.Login, model.Name), true);
            sec1.Add(followers);


            var events = new StyledStringElement("Events".t(), () => NavigationController.PushViewController(new RepoEventsViewController(model.Owner.Login, model.Name), true), Images.Event);

            var sec2 = new Section { events };

            if (model.HasIssues)
                sec2.Add(new StyledStringElement("Issues".t(), () => NavigationController.PushViewController(new IssuesViewController(model.Owner.Login, model.Name), true), Images.Flag));

            if (viewModel.Readme != null)
                sec2.Add(new StyledStringElement("Readme".t(), () => NavigationController.PushViewController(new ReadmeViewController(model.Owner.Login, model.Name), true), Images.File));

            if (model.HasWiki)
                sec2.Add(new StyledStringElement("Wiki".t(), () => NavigationController.PushViewController(new WikiViewController(model.Owner.Login, model.Name), true), Images.Pencil));

            var sec3 = new Section
            {
                new StyledStringElement("Changes".t(), () => NavigationController.PushViewController(new ChangesetViewController(model.Owner.Login, model.Name), true), Images.Changes),
                new StyledStringElement("Pull Requests".t(), () => NavigationController.PushViewController(new PullRequestsViewController(model.Owner.Login, model.Name), true), Images.Branch),
                new StyledStringElement("Branches".t(), () => NavigationController.PushViewController(new BranchesViewController(model.Owner.Login, model.Name), true), Images.Branch),
                new StyledStringElement("Tags".t(), () => NavigationController.PushViewController(new TagsViewController(model.Owner.Login, model.Name), true), Images.Tag)
            };

            root.Add(new[] { sec1, sec2, sec3 });

            if (!string.IsNullOrEmpty(model.Homepage))
            {
                var web = new StyledStringElement("Website".t(), () => UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(model.Homepage)), Images.Webpage);
                root.Add(new Section { web });
            }

            Root = root;
            NavigationItem.RightBarButtonItem.Enabled = true;
        }

        public void UpdatedImage(Uri uri)
        {
            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
            if (_header.Image != null)
                _header.SetNeedsDisplay();
        }

    }
}