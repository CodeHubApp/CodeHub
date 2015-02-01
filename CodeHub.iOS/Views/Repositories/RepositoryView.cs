using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryView : BaseDialogViewController<RepositoryViewModel>
    {
        private readonly SplitButtonElement _split = new SplitButtonElement();
        private readonly SplitViewElement[] _splitElements = new SplitViewElement[3];
        private readonly StyledStringElement _ownerElement;
        private readonly Section _sourceSection;

        public RepositoryView()
        {
            HeaderView.Image = Images.LoginUserUnknown;

            _sourceSection = new Section
            {
                new DialogStringElement("Commits", () => ViewModel.GoToCommitsCommand.ExecuteIfCan(), Images.Commit),
                new DialogStringElement("Pull Requests", () => ViewModel.GoToPullRequestsCommand.ExecuteIfCan(), Images.PullRequest),
                new DialogStringElement("Source", () => ViewModel.GoToSourceCommand.ExecuteIfCan(), Images.Code),
            };

            _ownerElement = new StyledStringElement("Owner", string.Empty) { Image = Images.Person,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
            _ownerElement.Tapped += () => ViewModel.GoToOwnerCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.Repository)
                .Subscribe(x => _ownerElement.Value = x == null ? string.Empty : x.Owner.Login);

            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);
            this.WhenAnyValue(x => x.ViewModel.GoToOwnerCommand).Subscribe(x => 
                HeaderView.ImageButtonAction = x != null ? new Action(() => ViewModel.GoToOwnerCommand.ExecuteIfCan()) : null);

            _splitElements[0] = new SplitViewElement();
            _splitElements[0].Button1 = new SplitViewElement.SplitButton(Images.Lock, string.Empty);
            _splitElements[0].Button2 = new SplitViewElement.SplitButton(Images.Package, string.Empty);

            _splitElements[1] = new SplitViewElement();
            _splitElements[1].Button1 = new SplitViewElement.SplitButton(Images.IssueOpened, string.Empty, () => ViewModel.GoToIssuesCommand.ExecuteIfCan());
            _splitElements[1].Button2 = new SplitViewElement.SplitButton(Images.Organization, string.Empty, () => ViewModel.GoToContributors.ExecuteIfCan());

            _splitElements[2] = new SplitViewElement();
            _splitElements[2].Button1 = new SplitViewElement.SplitButton(Images.Tag, string.Empty, () => ViewModel.GoToReleasesCommand.ExecuteIfCan());
            _splitElements[2].Button2 = new SplitViewElement.SplitButton(Images.Branch, string.Empty, () => ViewModel.GoToBranchesCommand.ExecuteIfCan());

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsStarred).Where(x => x.HasValue))
                .Switch()
                .Subscribe(x => HeaderView.SetSubImage(x.Value ? Images.Star.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) : null));

            this.WhenAnyValue(x => x.ViewModel.RepositoryName)
                .Subscribe(x => HeaderView.Text = x);

            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.Branches)
                .SubscribeSafe(x =>
                {
                    if (x == null)
                        _splitElements[2].Button2.Text = "- Branches";
                    else
                        _splitElements[2].Button2.Text = (x.Count >= 100 ? "100+" : x.Count.ToString()) + (x.Count == 1 ? " Branch" : " Branches");
                });

            this.WhenAnyValue(x => x.ViewModel.Contributors)
                .SubscribeSafe(x =>
                {
                    if (x == null)
                        _splitElements[1].Button2.Text = "- Contributors";
                    else
                        _splitElements[1].Button2.Text = (x >= 100 ? "100+" : x.ToString()) + (x == 1 ? " Contributor" : " Contributors");
                });


            this.WhenAnyValue(x => x.ViewModel.Releases)
                .SubscribeSafe(x =>
                {
                    if (x == null)
                        _splitElements[2].Button1.Text = "- Releases";
                    else
                        _splitElements[2].Button1.Text = (x >= 100 ? "100+" : x.ToString()) + (x == 1 ? " Release" : " Releases");
                });

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var stargazers = _split.AddButton("Stargazers", "-", () => ViewModel.GoToStargazersCommand.ExecuteIfCan());
            var watchers = _split.AddButton("Watchers", "-", () => ViewModel.GoToWatchersCommand.ExecuteIfCan());
            var forks = _split.AddButton("Forks", "-", () => ViewModel.GoToForksCommand.ExecuteIfCan());

            Root.Reset(new Section { _split });

            // Not very efficient but it'll work for now.
            ViewModel.WhenAnyValue(x => x.Readme).IsNotNull()
                .Select(_ => ViewModel.Repository).IsNotNull().Subscribe(_ => Render());

            ViewModel.WhenAnyValue(x => x.Stargazers).Subscribe(x =>
                stargazers.Text = x.HasValue ? x.ToString() : "-");

            ViewModel.WhenAnyValue(x => x.Watchers).Subscribe(x =>
                watchers.Text = x.HasValue ? x.ToString() : "-");

            ViewModel.WhenAnyValue(x => x.Repository).Where(x => x != null).Subscribe(x =>
            {
                HeaderView.ImageUri = x.Owner.AvatarUrl;
                HeaderView.SubText = x.Description;
                forks.Text = x.ForksCount.ToString();
                _splitElements[0].Button1.Text = x.Private ? "Private" : "Public";
                _splitElements[0].Button2.Text = x.Language ?? "N/A";
                _splitElements[1].Button1.Text = x.OpenIssues + (x.OpenIssues == 1 ? " Issue" : " Issues");
                Render();
            });
        }

        private void Render()
        {
            var model = ViewModel.Repository;
            var sec1 = new Section();
            sec1.Add(_splitElements);
            sec1.Add(_ownerElement);

            if (model.Parent != null)
            {
                var parent = new StyledStringElement("Forked From", model.Parent.FullName) { Image = Images.Fork,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
                parent.Tapped += () => ViewModel.GoToForkParentCommand.Execute(model.Parent);
                sec1.Add(parent);
            }

            var events = new DialogStringElement("Events", ViewModel.GoToEventsCommand.ExecuteIfCan, Images.Rss);
            var sec2 = new Section { events };

            if (model.HasIssues)
                sec2.Add(new DialogStringElement("Issues", ViewModel.GoToIssuesCommand.ExecuteIfCan, Images.IssueOpened));

            if (ViewModel.Readme != null)
                sec2.Add(new DialogStringElement("Readme", ViewModel.GoToReadmeCommand.ExecuteIfCan, Images.Book));

            Root.Reset(new Section { _split }, sec1, sec2, _sourceSection);

            if (!string.IsNullOrEmpty(model.Homepage))
            {
                Root.Add(new Section { 
                    new DialogStringElement("Website", ViewModel.GoToHomepageCommand.ExecuteIfCan, Images.Globe)
                });
            }
        }
    }
}