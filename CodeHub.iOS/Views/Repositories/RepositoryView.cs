using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using UIKit;
using CodeHub.iOS.DialogElements;

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
			var pinButton = sheet.AddButton(ViewModel.IsPinned ? "Unpin from Slideout Menu" : "Pin to Slideout Menu");
            var starButton = sheet.AddButton(ViewModel.IsStarred.Value ? "Unstar This Repo" : "Star This Repo");
            var watchButton = sheet.AddButton(ViewModel.IsWatched.Value ? "Unwatch This Repo" : "Watch This Repo");
            var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = sheet.AddButton("Cancel");
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

        SplitViewElement _split1 = new SplitViewElement(Octicon.Lock.ToImage(), Octicon.Package.ToImage());
        SplitViewElement _split2 = new SplitViewElement(Octicon.IssueOpened.ToImage(), Octicon.GitBranch.ToImage());
        SplitViewElement _split3 = new SplitViewElement(Octicon.Calendar.ToImage(), Octicon.Tools.ToImage());

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

            //Calculate the best representation of the size
            string size;
            if (model.Size / 1024f < 1)
                size = string.Format("{0:0.##}KB", model.Size);
            else if ((model.Size / 1024f / 1024f) < 1)
                size = string.Format("{0:0.##}MB", model.Size / 1024f);
            else
                size = string.Format("{0:0.##}GB", model.Size / 1024f / 1024f);

            _split1.Button1.Text = model.Private ? "Private" : "Public";
            _split1.Button2.Text = model.Language ?? "N/A";
            sec1.Add(_split1);

            _split2.Button1.Text = model.OpenIssues + (model.OpenIssues == 1 ? " Issue" : " Issues");
            _split2.Button2.Text = branches + (branches == 1 ? " Branch" : " Branches");
            sec1.Add(_split2);

            _split3.Button1.Text = (model.CreatedAt).ToString("MM/dd/yy");
            _split3.Button2.Text = size;
            sec1.Add(_split3);

            var owner = new StyledStringElement("Owner", model.Owner.Login) { Image = Octicon.Person.ToImage(),  Accessory = UITableViewCellAccessory.DisclosureIndicator };
			owner.Tapped += () => ViewModel.GoToOwnerCommand.Execute(null);
            sec1.Add(owner);

            if (model.Parent != null)
            {
                var parent = new StyledStringElement("Forked From", model.Parent.FullName) { Image = Octicon.RepoForked.ToImage(),  Accessory = UITableViewCellAccessory.DisclosureIndicator };
				parent.Tapped += () => ViewModel.GoToForkParentCommand.Execute(model.Parent);
                sec1.Add(parent);
            }

            var events = new StyledStringElement("Events", () => ViewModel.GoToEventsCommand.Execute(null), Octicon.Rss.ToImage());
            var sec2 = new Section { events };

            if (model.HasIssues)
            {
                sec2.Add(new StyledStringElement("Issues", () => ViewModel.GoToIssuesCommand.Execute(null), Octicon.IssueOpened.ToImage()));
            }

            if (ViewModel.Readme != null)
                sec2.Add(new StyledStringElement("Readme", () => ViewModel.GoToReadmeCommand.Execute(null), Octicon.Book.ToImage()));

            var sec3 = new Section
            {
                new StyledStringElement("Commits", () => ViewModel.GoToCommitsCommand.Execute(null), Octicon.GitCommit.ToImage()),
                new StyledStringElement("Pull Requests", () => ViewModel.GoToPullRequestsCommand.Execute(null), Octicon.GitPullRequest.ToImage()),
                new StyledStringElement("Source", () => ViewModel.GoToSourceCommand.Execute(null), Octicon.Code.ToImage()),
            };

            root.Add(new[] { sec1, sec2, sec3 });

            if (!string.IsNullOrEmpty(model.Homepage))
            {
                var web = new StyledStringElement("Website", () => ViewModel.GoToUrlCommand.Execute(model.Homepage), Octicon.Globe.ToImage());
                root.Add(new Section { web });
            }

            Root = root;
        }
    }
}