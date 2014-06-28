using System;
using System.Reactive.Linq;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeStash.iOS.Views;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryView : ViewModelDialogView<RepositoryViewModel>
    {
        private ImageAndTitleHeaderView _header;
        private UIActionSheet _actionSheet;
        private SplitButtonElement _split;

        protected override void Scrolled(System.Drawing.PointF point)
        {
            if (point.Y > 0)
            {
                NavigationController.NavigationBar.ShadowImage = null;
            }
            else
            {
                if (NavigationController.NavigationBar.ShadowImage == null)
                    NavigationController.NavigationBar.ShadowImage = new UIImage();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.NavigationBar.ShadowImage = null;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.NavigationBar.ShadowImage = new UIImage();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.Repository, x => x != null));

            TableView.SectionHeaderHeight = 0;
            RefreshControl.TintColor = UIColor.LightGray;

            _header = new ImageAndTitleHeaderView 
            { 
                Text = ViewModel.RepositoryName,
                BackgroundColor = NavigationController.NavigationBar.BackgroundColor,
                TextColor = UIColor.White,
                SubTextColor = UIColor.LightGray,
                RoundedImage = false,
                ImageTint = UIColor.White
            };

            var topBackgroundView = this.CreateTopBackground(_header.BackgroundColor);
            topBackgroundView.Hidden = true;


            _split = new SplitButtonElement();
            var stargazers = _split.AddButton("Stargazers", "-", () => ViewModel.GoToStargazersCommand.ExecuteIfCan());
            var watchers = _split.AddButton("Watchers", "-", () => ViewModel.GoToWatchersCommand.ExecuteIfCan());
            var collaborators = _split.AddButton("Contributors", "-", () => ViewModel.GoToCollaboratorsCommand.ExecuteIfCan());

            ViewModel.WhenAnyValue(x => x.Repository).Where(x => x != null).Subscribe(x =>
            {
                topBackgroundView.Hidden = false;
				ViewModel.ImageUri = (x.Fork ? Images.GitHubRepoForkUrl : Images.GitHubRepoUrl);
                _header.SubText = "Updated " + (ViewModel.Repository.UpdatedAt).ToDaysAgo();
                stargazers.Text = x.StargazersCount.ToString();
                watchers.Text = x.SubscribersCount.ToString();
                Render();
            });

            ViewModel.WhenAnyValue(x => x.Collaborators).Where(x => x.HasValue).Subscribe(x => collaborators.Text = x.ToString());

            ViewModel.WhenAnyValue(x => x.ImageUri).Where(x => x != null).Subscribe(x => 
            {
                _header.Image = UIImage.FromFile(x.LocalPath).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            });

            ViewModel.WhenAnyValue(x => x.Readme).Where(x => x != null).Subscribe(_ =>
            {
                // Not very efficient but it'll work for now.
                if (ViewModel.Repository != null)
                    Render();
            });
        }

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
            Title = ViewModel.RepositoryName;
            NavigationController.NavigationBar.ShadowImage = new UIImage();
		}

        private void ShowExtraMenu()
        {
            var repoModel = ViewModel.Repository;
            if (repoModel == null || ViewModel.IsStarred == null || ViewModel.IsWatched == null)
                return;

            _actionSheet = new UIActionSheet(repoModel.Name);
            var pinButton = _actionSheet.AddButton(ViewModel.IsPinned ? "Unpin from Slideout Menu" : "Pin to Slideout Menu");
            var starButton = _actionSheet.AddButton(ViewModel.IsStarred.Value ? "Unstar This Repo" : "Star This Repo");
            var watchButton = _actionSheet.AddButton(ViewModel.IsWatched.Value ? "Unwatch This Repo" : "Watch This Repo");
            //var forkButton = sheet.AddButton("Fork Repository");
            var showButton = _actionSheet.AddButton("Show in GitHub");
            var cancelButton = _actionSheet.AddButton("Cancel");
            _actionSheet.CancelButtonIndex = cancelButton;
            _actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
            _actionSheet.Clicked += (s, e) => {
                // Pin to menu
                if (e.ButtonIndex == pinButton)
                    ViewModel.PinCommand.ExecuteIfCan();
                else if (e.ButtonIndex == starButton)
                    ViewModel.ToggleStarCommand.ExecuteIfCan();
                else if (e.ButtonIndex == watchButton)
                    ViewModel.ToggleWatchCommand.ExecuteIfCan();
                else if (e.ButtonIndex == showButton)
					ViewModel.GoToHtmlUrlCommand.ExecuteIfCan();

                _actionSheet = null;
            };

            _actionSheet.ShowInView(View);
        }

        private void Render()
        {
            var model = ViewModel.Repository;
            var root = new RootElement(ViewModel.RepositoryName) { UnevenRows = true };
            root.Add(new Section(_header) { _split });
            var sec1 = new Section();

            if (!string.IsNullOrEmpty(model.Description) && !string.IsNullOrWhiteSpace(model.Description))
            {
                var element = new MultilinedElement(model.Description)
                {
                    BackgroundColor = UIColor.White,
                    CaptionColor = Theme.CurrentTheme.MainTitleColor, 
                    ValueColor = Theme.CurrentTheme.MainTextColor
                };
                element.CaptionColor = element.ValueColor;
                element.CaptionFont = element.ValueFont;
                sec1.Add(element);
            }

            //Calculate the best representation of the size
            string size;
            if (model.Size / 1024f < 1)
                size = string.Format("{0:0.##}KB", model.Size);
            else if ((model.Size / 1024f / 1024f) < 1)
                size = string.Format("{0:0.##}MB", model.Size / 1024f);
            else
                size = string.Format("{0:0.##}GB", model.Size / 1024f / 1024f);

            var splitElement1 = new SplitElement();
            splitElement1.Button1 = new SplitElement.SplitButton(model.Private ? Images.Locked : Images.Unlocked, model.Private ? "Private" : "Public");
            splitElement1.Button2 = new SplitElement.SplitButton(Images.Language, model.Language ?? "N/A");
            sec1.Add(splitElement1);

            var splitElement2 = new SplitElement();
            splitElement2.Button1 = new SplitElement.SplitButton(Images.Flag, model.OpenIssues + (model.OpenIssues == 1 ? " Issue" : " Issues"));
            splitElement2.Button2 = new SplitElement.SplitButton(Images.Fork, model.Forks + (model.Forks == 1 ? " Fork" : " Forks"));
            sec1.Add(splitElement2);

            var splitElement3 = new SplitElement();
            splitElement3.Button1 = new SplitElement.SplitButton(Images.Create, (model.CreatedAt.ToLocalTime()).ToString("MM/dd/yy"));
            splitElement3.Button2 = new SplitElement.SplitButton(Images.Size, size);
            sec1.Add(splitElement3);

            var owner = new StyledStringElement("Owner", model.Owner.Login) { Image = Images.Person,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
            owner.Tapped += () => ViewModel.GoToOwnerCommand.ExecuteIfCan();
            sec1.Add(owner);

            if (model.Parent != null)
            {
                var parent = new StyledStringElement("Forked From", model.Parent.FullName) { Image = Images.Fork,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
                parent.Tapped += () => ViewModel.GoToForkParentCommand.Execute(model.Parent);
                sec1.Add(parent);
            }

            var events = new StyledStringElement("Events", () => ViewModel.GoToEventsCommand.ExecuteIfCan(), Images.Event);
            var sec2 = new Section { events };

            if (model.HasIssues)
            {
                sec2.Add(new StyledStringElement("Issues", () => ViewModel.GoToIssuesCommand.ExecuteIfCan(), Images.Flag));
            }

            if (ViewModel.Readme != null)
                sec2.Add(new StyledStringElement("Readme", () => ViewModel.GoToReadmeCommand.ExecuteIfCan(), Images.File));

            var sec3 = new Section
            {
                new StyledStringElement("Commits", () => ViewModel.GoToCommitsCommand.ExecuteIfCan(), Images.Commit),
                new StyledStringElement("Pull Requests", () => ViewModel.GoToPullRequestsCommand.ExecuteIfCan(), Images.Hand),
                new StyledStringElement("Source", () => ViewModel.GoToSourceCommand.ExecuteIfCan(), Images.Script),
            };

            root.Add(new[] { sec1, sec2, sec3 });

            if (!string.IsNullOrEmpty(model.Homepage))
            {
                var web = new StyledStringElement("Website", () => ViewModel.GoToUrlCommand.Execute(model.Homepage), Images.Webpage);
                root.Add(new Section { web });
            }

            Root = root;
        }
    }
}