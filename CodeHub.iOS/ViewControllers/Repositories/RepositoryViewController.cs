using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoryViewController : BaseDialogViewController<RepositoryViewModel>
    {
        private readonly SplitButtonElement _split = new SplitButtonElement();
        private readonly SplitViewElement[] _splitElements = new SplitViewElement[3];
        private StringElement _ownerElement;
        private Section _sourceSection;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;

            _sourceSection = new Section
            {
                new StringElement("Commits", () => ViewModel.GoToCommitsCommand.ExecuteIfCan(), Octicon.GitCommit.ToImage()),
                new StringElement("Pull Requests", () => ViewModel.GoToPullRequestsCommand.ExecuteIfCan(), Octicon.GitPullRequest.ToImage()),
                new StringElement("Source", () => ViewModel.GoToSourceCommand.ExecuteIfCan(), Octicon.Code.ToImage()),
            };

            _ownerElement = new StringElement("Owner", string.Empty) { Image = Octicon.Person.ToImage() };
            _ownerElement.Tapped += () => ViewModel.GoToOwnerCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.Repository)
                .Subscribe(x => _ownerElement.Value = x == null ? string.Empty : x.Owner.Login);

            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);
            this.WhenAnyValue(x => x.ViewModel.GoToOwnerCommand).Subscribe(x => 
                HeaderView.ImageButtonAction = x != null ? new Action(() => ViewModel.GoToOwnerCommand.ExecuteIfCan()) : null);

            _splitElements[0] = new SplitViewElement();
            _splitElements[0].Button1 = new SplitViewElement.SplitButton(Octicon.Lock.ToImage(), string.Empty);
            _splitElements[0].Button2 = new SplitViewElement.SplitButton(Octicon.Package.ToImage(), string.Empty);

            _splitElements[1] = new SplitViewElement();
            _splitElements[1].Button1 = new SplitViewElement.SplitButton(Octicon.IssueOpened.ToImage(), string.Empty, () => ViewModel.GoToIssuesCommand.ExecuteIfCan());
            _splitElements[1].Button2 = new SplitViewElement.SplitButton(Octicon.Organization.ToImage(), string.Empty, () => ViewModel.GoToContributors.ExecuteIfCan());

            _splitElements[2] = new SplitViewElement();
            _splitElements[2].Button1 = new SplitViewElement.SplitButton(Octicon.Tag.ToImage(), string.Empty, () => ViewModel.GoToReleasesCommand.ExecuteIfCan());
            _splitElements[2].Button2 = new SplitViewElement.SplitButton(Octicon.GitBranch.ToImage(), string.Empty, () => ViewModel.GoToBranchesCommand.ExecuteIfCan());

            var stargazers = _split.AddButton("Stargazers", "-", () => ViewModel.GoToStargazersCommand.ExecuteIfCan());
            var watchers = _split.AddButton("Watchers", "-", () => ViewModel.GoToWatchersCommand.ExecuteIfCan());
            var forks = _split.AddButton("Forks", "-", () => ViewModel.GoToForksCommand.ExecuteIfCan());

            this.WhenAnyValue(x => x.ViewModel.Stargazers)
                .Select(x => x != null ? x.ToString() : "-")
                .Subscribe(x => stargazers.Text = x);

            this.WhenAnyValue(x => x.ViewModel.Watchers)
                .Select(x => x != null ? x.ToString() : "-")
                .Subscribe(x => watchers.Text = x);

            this.WhenAnyValue(x => x.ViewModel.Repository.ForksCount)
                .Subscribe(x => forks.Text = x.ToString());

            this.WhenAnyValue(x => x.ViewModel.Repository)
                .IsNotNull()
                .Subscribe(x =>
                    {
                        _splitElements[0].Button1.Text = x.Private ? "Private" : "Public";
                        _splitElements[0].Button2.Text = x.Language ?? "N/A";
                        _splitElements[1].Button1.Text = x.OpenIssuesCount + (x.OpenIssuesCount == 1 ? " Issue" : " Issues");
                    });

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsStarred).Where(x => x.HasValue))
                .Switch()
                .Subscribe(x => HeaderView.SetSubImage(x.Value ? Octicon.Star.ToImage() : null));

            this.WhenAnyValue(x => x.ViewModel.RepositoryName)
                .Subscribe(x => HeaderView.Text = x);

            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.Branches)
                .Select(x => x == null ? "- Branches" : (x.Count >= 100 ? "100+" : x.Count.ToString()) + (x.Count == 1 ? " Branch" : " Branches"))
                .SubscribeSafe(x => _splitElements[2].Button2.Text = x);

            this.WhenAnyValue(x => x.ViewModel.Contributors)
                .Select(x => x == null ? "- Contributors" : (x >= 100 ? "100+" : x.ToString()) + (x == 1 ? " Contributor" : " Contributors"))
                .SubscribeSafe(x => _splitElements[1].Button2.Text = x);

            this.WhenAnyValue(x => x.ViewModel.Releases)
                .Select(x => x == null ? "- Releases" : (x >= 100 ? "100+" : x.ToString()) + (x == 1 ? " Release" : " Releases"))
                .SubscribeSafe(x => _splitElements[2].Button1.Text = x);

            this.WhenAnyValue(x => x.ViewModel.Description)
                .Subscribe(x => {
                    HeaderView.SubText = x;
                    RefreshHeaderView();
                });

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Subscribe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown));

            this.WhenAnyValue(x => x.ViewModel.Repository)
                .IsNotNull()
                .Subscribe(_ => Render());

            this.WhenAnyValue(x => x.ViewModel.Readme)
                .Where(x => x != null && ViewModel.Repository != null)
                .Subscribe(_ => Render());
        }

        private void Render()
        {
            var model = ViewModel.Repository;
            var sec1 = new Section();
            sec1.Add(_splitElements);
            sec1.Add(_ownerElement);

            if (model.Parent != null)
            {
                var parent = new StringElement("Forked From", model.Parent.FullName) { Image = Octicon.RepoForked.ToImage() };
                parent.Tapped += () => ViewModel.GoToForkParentCommand.Execute(model.Parent);
                sec1.Add(parent);
            }

            var events = new StringElement("Events", ViewModel.GoToEventsCommand.ExecuteIfCan, Octicon.Rss.ToImage());
            var sec2 = new Section { events };

            if (model.HasIssues)
                sec2.Add(new StringElement("Issues", ViewModel.GoToIssuesCommand.ExecuteIfCan, Octicon.IssueOpened.ToImage()));

            if (ViewModel.Readme != null)
                sec2.Add(new StringElement("Readme", ViewModel.GoToReadmeCommand.ExecuteIfCan, Octicon.Book.ToImage()));

            Root.Reset(new Section { _split }, sec1, sec2, _sourceSection);

            if (!string.IsNullOrEmpty(model.Homepage))
            {
                Root.Add(new Section { 
                    new StringElement("Website", ViewModel.GoToHomepageCommand.ExecuteIfCan, Octicon.Globe.ToImage())
                });
            }
        }
    }
}