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
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryView : PrettyDialogViewController
    {
        private readonly IFeaturesService _featuresService = Mvx.Resolve<IFeaturesService>();
        private IDisposable _privateView;

        public new RepositoryViewModel ViewModel
        {
            get { return (RepositoryViewModel)base.ViewModel; }
            protected set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = ViewModel.Username;
            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = ViewModel.RepositoryName;
            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)).Take(1))
                .Switch()
                .Select(_ => ViewModel.Bind(x => x.IsStarred, true).Where(x => x.HasValue))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HeaderView.SetSubImage(x.Value ? Octicon.Star.ToImage() : null));

            var actionButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action) { Enabled = false };

            _split = new SplitButtonElement();
            _stargazers = _split.AddButton("Stargazers", "-");
            _watchers = _split.AddButton("Watchers", "-");
            _forks = _split.AddButton("Forks", "-");

            OnActivation(d =>
            {
                d(_stargazers.Clicked.BindCommand(ViewModel.GoToStargazersCommand));
                d(_watchers.Clicked.BindCommand(ViewModel.GoToWatchersCommand));
                d(_forks.Clicked.BindCommand(ViewModel.GoToForkedCommand));
                d(actionButton.GetClickedObservable().Subscribe(_ => ShowExtraMenu()));

                d(_eventsElement.Clicked.BindCommand(ViewModel.GoToEventsCommand));
                d(_ownerElement.Clicked.BindCommand(ViewModel.GoToOwnerCommand));

                d(_commitsElement.Clicked.BindCommand(ViewModel.GoToCommitsCommand));
                d(_pullRequestsElement.Clicked.BindCommand(ViewModel.GoToPullRequestsCommand));
                d(_sourceElement.Clicked.BindCommand(ViewModel.GoToSourceCommand));

                d(ViewModel.Bind(x => x.Branches, true).Subscribe(_ => Render()));
                d(ViewModel.Bind(x => x.Readme, true).Subscribe(_ => Render()));

                d(_forkElement.Value.Clicked.Select(x => ViewModel.Repository.Parent).BindCommand(ViewModel.GoToForkParentCommand));
                d(_issuesElement.Value.Clicked.BindCommand(ViewModel.GoToIssuesCommand));
                d(_readmeElement.Value.Clicked.BindCommand(ViewModel.GoToReadmeCommand));
                d(_websiteElement.Value.Clicked.Select(x => ViewModel.Repository.Homepage).BindCommand(ViewModel.GoToUrlCommand));

                d(HeaderView.Clicked.BindCommand(ViewModel.GoToOwnerCommand));

                d(ViewModel.Bind(x => x.Repository, true).Where(x => x != null).Subscribe(x =>
                {
                    if (x.Private && !_featuresService.IsProEnabled)
                    {
                        if (_privateView == null)
                            _privateView = this.ShowPrivateView();
                        actionButton.Enabled = false;
                    }
                    else
                    {
                        actionButton.Enabled = true;
                        _privateView?.Dispose();
                    }

                    ViewModel.ImageUrl = x.Owner?.AvatarUrl;
                    HeaderView.SubText = Emojis.FindAndReplace(x.Description);
                    HeaderView.SetImage(x.Owner?.AvatarUrl, Images.Avatar);
                    Render();
                    RefreshHeaderView();
                }));
            });
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
            var shareButton = sheet.AddButton("Share");
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
                else if (e.ButtonIndex == shareButton)
                {
                    AlertDialogService.ShareUrl(ViewModel.Repository.HtmlUrl, NavigationItem.RightBarButtonItem);
                }

                sheet.Dispose();
            };

            sheet.ShowFrom(NavigationItem.RightBarButtonItem, true);
        }

        SplitViewElement _split1 = new SplitViewElement(Octicon.Lock.ToImage(), Octicon.Package.ToImage());
        SplitViewElement _split2 = new SplitViewElement(Octicon.IssueOpened.ToImage(), Octicon.GitBranch.ToImage());
        SplitViewElement _split3 = new SplitViewElement(Octicon.Calendar.ToImage(), Octicon.Tools.ToImage());
        SplitButtonElement _split = new SplitButtonElement();
        SplitButtonElement.Button _stargazers;
        SplitButtonElement.Button _watchers;
        SplitButtonElement.Button _forks;
        private readonly StringElement _ownerElement = new StringElement("Owner", string.Empty) { Image = Octicon.Person.ToImage() };
        private readonly StringElement _eventsElement = new StringElement("Events", Octicon.Rss.ToImage());
        private readonly StringElement _commitsElement = new StringElement("Commits", Octicon.GitCommit.ToImage());
        private readonly StringElement _pullRequestsElement = new StringElement("Pull Requests", Octicon.GitPullRequest.ToImage());
        private readonly StringElement _sourceElement = new StringElement("Source", Octicon.Code.ToImage());

        private readonly Lazy<StringElement> _forkElement;
        private readonly Lazy<StringElement> _issuesElement;
        private readonly Lazy<StringElement> _readmeElement;
        private readonly Lazy<StringElement> _websiteElement;


        public RepositoryView()
        {
            _forkElement = new Lazy<StringElement>(() => new StringElement("Forked From", string.Empty) { Image = Octicon.RepoForked.ToImage() });
            _issuesElement = new Lazy<StringElement>(() => new StringElement("Issues", Octicon.IssueOpened.ToImage()));
            _readmeElement = new Lazy<StringElement>(() => new StringElement("Readme", Octicon.Book.ToImage()));
            _websiteElement = new Lazy<StringElement>(() => new StringElement("Website", Octicon.Globe.ToImage()));
        }

        public void Render()
        {
            var model = ViewModel.Repository;
            var branches = ViewModel.Branches?.Count ?? 0;
            if (model == null)
                return;

            _stargazers.Text = model.StargazersCount.ToString();
            _watchers.Text = model.SubscribersCount.ToString();
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

            _ownerElement.Value = model.Owner.Login;
            sec1.Add(_ownerElement);

            if (model.Parent != null)
                sec1.Add(_forkElement.Value);

            var sec2 = new Section { _eventsElement };

            if (model.HasIssues)
                sec2.Add(_issuesElement.Value);

            if (ViewModel.Readme != null)
                sec2.Add(_readmeElement.Value);

            sections.Add(sec1);
            sections.Add(sec2);
            sections.Add(new Section { _commitsElement, _pullRequestsElement, _sourceElement });

            if (!string.IsNullOrEmpty(model.Homepage))
                sections.Add(new Section { _websiteElement.Value });

            Root.Reset(sections);
        }
    }
}