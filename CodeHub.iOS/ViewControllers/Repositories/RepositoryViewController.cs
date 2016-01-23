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
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var splitButton = new SplitButtonElement();
            var splitElements = new SplitViewElement[3];

            HeaderView.Image = Images.LoginUserUnknown;
            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)).Take(1))
                .Switch()
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsStarred).Where(x => x.HasValue))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HeaderView.SetSubImage(x.Value ? Octicon.Star.ToImage() : null));

            var events = new StringElement("Events", Octicon.Rss.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var issuesElement = new StringElement("Issues", Octicon.IssueOpened.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var commitsElement = new StringElement("Commits", Octicon.GitCommit.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var pullRequestsElement = new StringElement("Pull Requests", Octicon.GitPullRequest.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var sourceElement = new StringElement("Source", Octicon.Code.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var websiteElement = new StringElement("Website", Octicon.Globe.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var readmeElement = new StringElement("Readme", Octicon.Book.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var forkedElement = new StringElement("Fork", string.Empty) { 
                Image = Octicon.RepoForked.ToImage(),
                Font = StringElement.DefaultDetailFont,
                Accessory = UITableViewCellAccessory.DisclosureIndicator 
            };

            splitElements[0] = new SplitViewElement(Octicon.Lock.ToImage(), Octicon.Package.ToImage());
            splitElements[1] = new SplitViewElement(Octicon.IssueOpened.ToImage(), Octicon.Organization.ToImage());
            splitElements[2] = new SplitViewElement(Octicon.Tag.ToImage(), Octicon.GitBranch.ToImage());

            var stargazers = splitButton.AddButton("Stargazers", "-");
            var watchers = splitButton.AddButton("Watchers", "-");
            var forks = splitButton.AddButton("Forks", "-");

            var renderFunc = new Action(() => {
                var model = ViewModel.Repository;
                var sec1 = new Section();
                sec1.Add(splitElements);

                if (model.Parent != null)
                {
                    forkedElement.Value = model.Parent.FullName;
                    sec1.Add(forkedElement);
                }

                var sec2 = new Section { events };

                if (model.HasIssues)
                    sec2.Add(issuesElement);

                if (ViewModel.Readme != null)
                    sec2.Add(readmeElement);

                Root.Reset(new Section { splitButton }, sec1, sec2, new Section { commitsElement, pullRequestsElement, sourceElement });

                if (!string.IsNullOrEmpty(model.Homepage))
                {
                    Root.Add(new Section { websiteElement });
                }
            });

            OnActivation(d => {
                d(HeaderView.Clicked.InvokeCommand(ViewModel.GoToOwnerCommand));

                d(splitElements[1].Button1.Clicked.InvokeCommand(ViewModel.GoToIssuesCommand));
                d(splitElements[1].Button2.Clicked.InvokeCommand(ViewModel.GoToContributors));
                d(splitElements[2].Button1.Clicked.InvokeCommand(ViewModel.GoToReleasesCommand));
                d(splitElements[2].Button2.Clicked.InvokeCommand(ViewModel.GoToBranchesCommand));

                d(events.Clicked.InvokeCommand(ViewModel.GoToEventsCommand));
                d(issuesElement.Clicked.InvokeCommand(ViewModel.GoToIssuesCommand));
                d(websiteElement.Clicked.InvokeCommand(ViewModel.GoToHomepageCommand));
                d(forkedElement.Clicked.InvokeCommand(ViewModel.GoToForkParentCommand));
                d(readmeElement.Clicked.InvokeCommand(ViewModel.GoToReadmeCommand));

                d(stargazers.Clicked.InvokeCommand(ViewModel.GoToStargazersCommand));
                d(watchers.Clicked.InvokeCommand(ViewModel.GoToWatchersCommand));
                d(forks.Clicked.InvokeCommand(ViewModel.GoToForksCommand));

                d(commitsElement.Clicked.InvokeCommand(ViewModel.GoToCommitsCommand));
                d(pullRequestsElement.Clicked.InvokeCommand(ViewModel.GoToPullRequestsCommand));
                d(sourceElement.Clicked.InvokeCommand(ViewModel.GoToSourceCommand));

                d(this.WhenAnyValue(x => x.ViewModel.Stargazers)
                    .Select(x => x != null ? x.ToString() : "-")
                    .Subscribe(x => stargazers.Text = x));

                d(this.WhenAnyValue(x => x.ViewModel.Watchers)
                    .Select(x => x != null ? x.ToString() : "-")
                    .Subscribe(x => watchers.Text = x));

                d(this.WhenAnyValue(x => x.ViewModel.Repository.ForksCount)
                    .Subscribe(x => forks.Text = x.ToString()));

                d(this.WhenAnyValue(x => x.ViewModel.Repository)
                    .IsNotNull()
                    .Subscribe(x =>
                        {
                            splitElements[0].Button1.Text = x.Private ? "Private" : "Public";
                            splitElements[0].Button2.Text = x.Language ?? "N/A";
                            splitElements[1].Button1.Text = x.OpenIssuesCount + (x.OpenIssuesCount == 1 ? " Issue" : " Issues");
                        }));

                d(this.WhenAnyValue(x => x.ViewModel.RepositoryName)
                    .Subscribe(x => HeaderView.Text = x));

                d(this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Action, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.Branches)
                    .Select(x => x == null ? "Branches" : (x.Count >= 100 ? "100+" : x.Count.ToString()) + (x.Count == 1 ? " Branch" : " Branches"))
                    .SubscribeSafe(x => splitElements[2].Button2.Text = x));

                d(this.WhenAnyValue(x => x.ViewModel.Contributors)
                    .Select(x => x == null ? "Contributors" : (x >= 100 ? "100+" : x.ToString()) + (x == 1 ? " Contributor" : " Contributors"))
                    .SubscribeSafe(x => splitElements[1].Button2.Text = x));
                
                d(this.WhenAnyValue(x => x.ViewModel.Releases)
                    .Select(x => x == null ? "Releases" : (x >= 100 ? "100+" : x.ToString()) + (x == 1 ? " Release" : " Releases"))
                    .SubscribeSafe(x => splitElements[2].Button1.Text = x));

                d(this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => RefreshHeaderView(subtext: x)));

                d(this.WhenAnyValue(x => x.ViewModel.Avatar)
                    .Subscribe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown)));

                d(this.WhenAnyValue(x => x.ViewModel.Repository)
                    .IsNotNull()
                    .Subscribe(_ => renderFunc()));

                d(this.WhenAnyValue(x => x.ViewModel.Readme)
                    .Where(x => x != null && ViewModel.Repository != null)
                    .Subscribe(_ => renderFunc()));
            });
        }
    }
}