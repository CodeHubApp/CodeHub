using System;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using CodeHub.iOS.DialogElements;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.iOS.Services;
using CodeHub.Core.Utilities;
using CodeHub.iOS.ViewControllers.Users;
using CodeHub.iOS.ViewControllers.Source;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.Views.Source;
using System.Linq;
using Humanizer;
using System.Reactive;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoryViewController : PrettyDialogViewController
    {
        private readonly IFeaturesService _featuresService;
        private IDisposable _privateView;

        public new RepositoryViewModel ViewModel
        {
            get { return (RepositoryViewModel)base.ViewModel; }
            protected set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = ViewModel.RepositoryName;
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
                d(_watchers.Clicked
                  .Select(_ => ViewModel)
                  .Select(x => UsersViewController.CreateWatchersViewController(x.Username, x.RepositoryName))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(_stargazers.Clicked
                  .Select(_ => ViewModel)
                  .Select(x => UsersViewController.CreateStargazersViewController(x.Username, x.RepositoryName))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(actionButton.GetClickedObservable().Subscribe(ShowExtraMenu));

                d(_forks.Clicked.Subscribe(_ =>
                {
                    var vc = RepositoriesViewController.CreateForkedViewController(ViewModel.Username, ViewModel.RepositoryName);
                    NavigationController?.PushViewController(vc, true);
                }));

                d(_eventsElement.Clicked.BindCommand(ViewModel.GoToEventsCommand));

                d(_commitsElement.Clicked.Subscribe(_ => GoToCommits()));

                d(_pullRequestsElement.Clicked.BindCommand(ViewModel.GoToPullRequestsCommand));
                d(_sourceElement.Clicked.Subscribe(_ => GoToSourceCode()));

                d(ViewModel.Bind(x => x.Branches, true).Subscribe(_ => Render()));
                d(ViewModel.Bind(x => x.Readme, true).Subscribe(_ => Render()));

                d(ViewModel.Bind(x => x.Repository)
                  .Select(x => x?.Parent)
                  .Where(x => x != null)
                  .Subscribe(x => _forkElement.Value.Value = x.FullName));

                d(_forkElement
                  .Value.Clicked
                  .Select(x => ViewModel.Repository?.Parent)
                  .Subscribe(GoToForkedRepository));

                d(_issuesElement.Value.Clicked.BindCommand(ViewModel.GoToIssuesCommand));
                d(_readmeElement.Value.Clicked.Subscribe(_ => GoToReadme()));
                d(_websiteElement.Value.Clicked.Select(x => ViewModel.Repository.Homepage).BindCommand(ViewModel.GoToUrlCommand));

                d(HeaderView.Clicked.Merge(_ownerElement.Clicked.Select(_ => Unit.Default))
                  .Select(_ => new UserViewController(ViewModel.Username, ViewModel.Repository.Owner))
                  .Subscribe(x => this.PushViewController(x)));

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

        private void ShowExtraMenu(UIBarButtonItem barButtonItem)
        {
            var repoModel = ViewModel.Repository;
            if (repoModel == null || ViewModel.IsStarred == null || ViewModel.IsWatched == null)
                return;

            var sheet = new UIActionSheet();
            var pinButton = sheet.AddButton(ViewModel.IsPinned ? "Unpin from Slideout Menu" : "Pin to Slideout Menu");
            var starButton = sheet.AddButton(ViewModel.IsStarred.Value ? "Unstar This Repo" : "Star This Repo");
            var watchButton = sheet.AddButton(ViewModel.IsWatched.Value ? "Unwatch This Repo" : "Watch This Repo");
            var showButton = ViewModel?.Repository?.HtmlUrl != null ? sheet.AddButton("Show in GitHub") : -1;
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
                    AlertDialogService.Share(
                        repoModel.FullName,
                        repoModel.Description,
                        repoModel.HtmlUrl,
                        NavigationItem.RightBarButtonItem);
                }

                sheet.Dispose();
            };

            sheet.ShowFrom(barButtonItem, true);
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

        public static RepositoryViewController CreateCodeHubViewController()
            => new RepositoryViewController("codehubapp", "codehub");

        public RepositoryViewController(Octokit.Repository repository)
            : this(repository.Owner.Login, repository.Name, repository)
        {
        }

        public RepositoryViewController(
            string owner,
            string repositoryName,
            Octokit.Repository repository = null)
            : this()
        {
            ViewModel = new RepositoryViewModel();
            ViewModel.Init(new RepositoryViewModel.NavObject { Username = owner, Repository = repositoryName });
            ViewModel.Repository = repository;
        }

        private void GoToSourceCode()
        {
            var defaultBranch = ViewModel.Repository?.DefaultBranch;
            if (string.IsNullOrEmpty(defaultBranch))
                return;

            this.PushViewController(new SourceTreeViewController(
                ViewModel.Username,
                ViewModel.RepositoryName,
                null,
                defaultBranch,
                ShaType.Branch));
        }

        private void GoToReadme()
        {
            this.PushViewController(
                new ReadmeViewController(ViewModel.Username, ViewModel.RepositoryName, ViewModel.Readme));
        }

        private void GoToForkedRepository(Octokit.Repository repo)
        {
            if (repo == null)
                return;

            this.PushViewController(new RepositoryViewController(repo));
        }

        private void GoToCommits()
        {
            var owner = ViewModel.Username;
            var repo = ViewModel.RepositoryName;
            var branches = ViewModel.Branches;
            if (branches?.Count == 1)
            {
                var viewController = new ChangesetsView(owner, repo, branches.First().Name);
                this.PushViewController(viewController);
            }
            else
            {
                var viewController = new BranchesViewController(owner, repo, branches);
                viewController.BranchSelected.Subscribe(
                    branch => viewController.PushViewController(new ChangesetsView(owner, repo, branch.Name)));
                this.PushViewController(viewController);
            }
        }

        public RepositoryViewController()
        {
            _featuresService = Mvx.Resolve<IFeaturesService>();

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

            _split1.Button1.Text = model.Private ? "Private" : "Public";
            _split1.Button2.Text = model.Language ?? "N/A";
            sec1.Add(_split1);

            _split2.Button1.Text = "Issue".ToQuantity(model.OpenIssuesCount);
            _split2.Button2.Text = "Branch".ToQuantity(branches);
            sec1.Add(_split2);

            _split3.Button1.Text = (model.CreatedAt).ToString("MM/dd/yy");
            _split3.Button2.Text = model.Size.Bytes().ToString("#.#");
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