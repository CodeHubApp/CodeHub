using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using UIKit;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Repositories;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using System.Collections.Generic;
using CodeHub.iOS.Utilities;

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
            var features = Mvx.Resolve<IFeaturesService>();

            ViewModel.Bind(x => x.Repository).Subscribe(x =>
            {
                if (x.Private && !features.IsProEnabled)
                {
                    this.ShowPrivateView();
                    _actionButton.Enabled = false;
                }
                else
                {
                    _actionButton.Enabled = true;
                }

                ViewModel.ImageUrl = x.Owner?.AvatarUrl;
                HeaderView.SubText = Emojis.FindAndReplace(x.Description);
                HeaderView.SetImage(x.Owner?.AvatarUrl, Images.Avatar);
                Render();
                RefreshHeaderView();
            });

            ViewModel.Bind(x => x.Branches).Subscribe(_ => Render());
            ViewModel.Bind(x => x.Readme).Subscribe(_ => Render());

            _split = new SplitButtonElement();
            _stargazers = _split.AddButton("Stargazers", "-");
            _watchers = _split.AddButton("Watchers", "-");
            _forks = _split.AddButton("Forks", "-");

            OnActivation(d =>
            {
                d(_stargazers.Clicked.BindCommand(ViewModel.GoToStargazersCommand));
                d(_watchers.Clicked.BindCommand(ViewModel.GoToWatchersCommand));
                d(_forks.Clicked.BindCommand(ViewModel.GoToForkedCommand));
            });
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
        SplitButtonElement _split = new SplitButtonElement();
        SplitButtonElement.Button _stargazers;
        SplitButtonElement.Button _watchers;
        SplitButtonElement.Button _forks;

        public void Render()
        {
            var model = ViewModel.Repository;
            var branches = ViewModel.Branches?.Count ?? 0;
            if (model == null)
                return;

            _stargazers.Text = model.StargazersCount.ToString();
            _watchers.Text = model.WatchersCount.ToString();
            _forks.Text = model.ForksCount.ToString();

            Title = model.Name;
            ICollection<Section> sections = new LinkedList<Section>();

            sections.Add(new Section { _split });
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

            var owner = new StringElement("Owner", model.Owner.Login) { Image = Octicon.Person.ToImage(),  Accessory = UITableViewCellAccessory.DisclosureIndicator };
            owner.Clicked.Subscribe(_ => ViewModel.GoToOwnerCommand.Execute(null));
            sec1.Add(owner);

            if (model.Parent != null)
            {
                var parent = new StringElement("Forked From", model.Parent.FullName) { Image = Octicon.RepoForked.ToImage(),  Accessory = UITableViewCellAccessory.DisclosureIndicator };
                parent.Clicked.Subscribe(_ => ViewModel.GoToForkParentCommand.Execute(model.Parent));
                sec1.Add(parent);
            }

            var events = new StringElement("Events", Octicon.Rss.ToImage());
            events.Clicked.Subscribe(_ => ViewModel.GoToEventsCommand.Execute(null));
            var sec2 = new Section { events };

            if (model.HasIssues)
            {
                var issues = new StringElement("Issues", Octicon.IssueOpened.ToImage());
                issues.Clicked.Subscribe(_ => ViewModel.GoToIssuesCommand.Execute(null));
                sec2.Add(issues);
            }

            if (ViewModel.Readme != null)
            {
                var readme = new StringElement("Readme", Octicon.Book.ToImage());
                readme.Clicked.Subscribe(_ => ViewModel.GoToReadmeCommand.Execute(null));
                sec2.Add(readme);
            }

            var commits = new StringElement("Commits", Octicon.GitCommit.ToImage());
            commits.Clicked.Subscribe(_ => ViewModel.GoToCommitsCommand.Execute(null));

            var pullRequests = new StringElement("Pull Requests", Octicon.GitPullRequest.ToImage());
            pullRequests.Clicked.Subscribe(_ => ViewModel.GoToPullRequestsCommand.Execute(null));

            var source = new StringElement("Source", Octicon.Code.ToImage());
            source.Clicked.Subscribe(_ => ViewModel.GoToSourceCommand.Execute(null));

            sections.Add(sec1);
            sections.Add(sec2);
            sections.Add(new Section { commits, pullRequests, source });

            if (!string.IsNullOrEmpty(model.Homepage))
            {
                var web = new StringElement("Website", Octicon.Globe.ToImage());
                web.Clicked.Subscribe(_ => ViewModel.GoToUrlCommand.Execute(model.Homepage));
                sections.Add(new Section { web });
            }

            Root.Reset(sections);
        }
    }
}