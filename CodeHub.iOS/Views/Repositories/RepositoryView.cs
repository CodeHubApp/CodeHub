using System;
using CodeFramework.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using UIKit;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryView : PrettyDialogViewController
    {
        private readonly UIBarButtonItem _actionButton;

        public new RepositoryViewModel ViewModel
        {
            get { return (RepositoryViewModel)base.ViewModel; }
            protected set { base.ViewModel = value; }
        }

        public RepositoryView()
        {
            _actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu()) { Enabled = false };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = ViewModel.Username;
            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = ViewModel.RepositoryName;

            ViewModel.Bind(x => x.Repository, x =>
            {
                ViewModel.ImageUrl = x.Owner?.AvatarUrl;
                HeaderView.SubText = x.Description;
                HeaderView.SetImage(x.Owner?.AvatarUrl, Images.Avatar);
                _actionButton.Enabled = true;
                Render();
                RefreshHeaderView();
            });

            ViewModel.Bind(x => x.Branches, Render);

            ViewModel.Bind(x => x.Readme, Render);
        }

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
            NavigationItem.RightBarButtonItem = _actionButton;
		}

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

        private void ShowExtraMenu()
        {
            var repoModel = ViewModel.Repository;
            if (repoModel == null || ViewModel.IsStarred == null || ViewModel.IsWatched == null)
                return;

            var sheet = new UIActionSheet();
			var pinButton = sheet.AddButton(ViewModel.IsPinned ? "Unpin from Slideout Menu".t() : "Pin to Slideout Menu".t());
            var starButton = sheet.AddButton(ViewModel.IsStarred.Value ? "Unstar This Repo".t() : "Star This Repo".t());
            var watchButton = sheet.AddButton(ViewModel.IsWatched.Value ? "Unwatch This Repo".t() : "Watch This Repo".t());
            var showButton = sheet.AddButton("Show in GitHub".t());
            var cancelButton = sheet.AddButton("Cancel".t());
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
			sheet.Dismissed += (s, e) => {
                // Pin to menu
                if (e.ButtonIndex == pinButton)
                {
                    ViewModel.PinCommand.Execute(null);
                }
                else if (e.ButtonIndex == starButton)
                {
                    ViewModel.ToggleStarCommand.Execute(null);
                }
                else if (e.ButtonIndex == watchButton)
                {
                    ViewModel.ToggleWatchCommand.Execute(null);
                }
                else if (e.ButtonIndex == showButton)
                {
					ViewModel.GoToHtmlUrlCommand.Execute(null);
                }

                sheet.Dispose();
            };

            sheet.ShowInView(this.View);
        }

        public void Render()
        {
            var model = ViewModel.Repository;
            var branches = ViewModel.Branches?.Count ?? 0;
            if (model == null)
                return;

            var split = new SplitButtonElement();
            split.AddButton("Stargazers", model.StargazersCount.ToString(), () => ViewModel.GoToStargazersCommand.Execute(null));
            split.AddButton("Watchers", model.WatchersCount.ToString(), () => ViewModel.GoToWatchersCommand.Execute(null));
            split.AddButton("Forks", model.ForksCount.ToString(), () => ViewModel.GoToForkedCommand.Execute(null));

            Title = model.Name;
            var root = new RootElement(Title) { UnevenRows = true };

            root.Add(new Section() { split });
            var sec1 = new Section();

            sec1.Add(new SplitElement(new SplitElement.Row {
                Text1 = model.Private ? "Private".t() : "Public".t(),
                Image1 = model.Private ? Images.Locked : Images.Unlocked,
				Text2 = model.Language ?? "N/A",
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
                Text1 = model.OpenIssues + (model.OpenIssues == 1 ? " Issue".t() : " Issues".t()),
                Image1 = Images.Flag,
                Text2 = branches + (branches == 1 ? " Branches".t() : " Branches".t()),
                Image2 = Images.Branch
            }));

            sec1.Add(new SplitElement(new SplitElement.Row {
                Text1 = (model.CreatedAt).ToString("MM/dd/yy"),
                Image1 = Images.Create,
                Text2 = size,
                Image2 = Images.Size
            }));

            var owner = new StyledStringElement("Owner".t(), model.Owner.Login) { Image = Images.Person,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
			owner.Tapped += () => ViewModel.GoToOwnerCommand.Execute(null);
            sec1.Add(owner);

            if (model.Parent != null)
            {
                var parent = new StyledStringElement("Forked From".t(), model.Parent.FullName) { Image = Images.Fork,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
				parent.Tapped += () => ViewModel.GoToForkParentCommand.Execute(model.Parent);
                sec1.Add(parent);
            }

			var events = new StyledStringElement("Events".t(), () => ViewModel.GoToEventsCommand.Execute(null), Images.Event);
            var sec2 = new Section { events };

            if (model.HasIssues)
            {
                sec2.Add(new StyledStringElement("Issues".t(), () => ViewModel.GoToIssuesCommand.Execute(null), Images.Flag));
            }

            if (ViewModel.Readme != null)
				sec2.Add(new StyledStringElement("Readme".t(), () => ViewModel.GoToReadmeCommand.Execute(null), Images.File));

            var sec3 = new Section
            {
				new StyledStringElement("Commits".t(), () => ViewModel.GoToCommitsCommand.Execute(null), Images.Commit),
				new StyledStringElement("Pull Requests".t(), () => ViewModel.GoToPullRequestsCommand.Execute(null), Images.Hand),
				new StyledStringElement("Source".t(), () => ViewModel.GoToSourceCommand.Execute(null), Images.Script),
            };

            root.Add(new[] { sec1, sec2, sec3 });

            if (!string.IsNullOrEmpty(model.Homepage))
            {
				var web = new StyledStringElement("Website".t(), () => ViewModel.GoToUrlCommand.Execute(model.Homepage), Images.Webpage);
                root.Add(new Section { web });
            }

            Root = root;
        }
    }
}