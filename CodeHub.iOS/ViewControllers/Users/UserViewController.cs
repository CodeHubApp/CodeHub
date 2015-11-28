using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UserViewController : BaseDialogViewController<UserViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;
            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)))
                .Switch()
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsFollowing).Where(x => x.HasValue))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HeaderView.SetSubImage(x.Value ? Octicon.Star.ToImage() : null));

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-");
            var following = split.AddButton("Following", "-");
            var events = new StringElement("Events", Octicon.Rss.ToImage());
            var organizations = new StringElement("Organizations", Octicon.Organization.ToImage());
            var repos = new StringElement("Repositories", Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", Octicon.Gist.ToImage());
            var website = new StringElement("Website", Octicon.Globe.ToImage()) { Hidden = true };
            Root.Reset(new [] { new Section { split }, new Section { events, organizations, repos, gists }, new Section { website } });

            OnActivation(d => {
                d(followers.Clicked.InvokeCommand(ViewModel.GoToFollowersCommand));
                d(following.Clicked.InvokeCommand(ViewModel.GoToFollowingCommand));
                d(website.Clicked.InvokeCommand(ViewModel.GoToWebsiteCommand));
                d(events.Clicked.InvokeCommand(ViewModel.GoToEventsCommand));
                d(organizations.Clicked.InvokeCommand(ViewModel.GoToOrganizationsCommand));
                d(repos.Clicked.InvokeCommand(ViewModel.GoToRepositoriesCommand));
                d(gists.Clicked.InvokeCommand(ViewModel.GoToGistsCommand));

                d(this.WhenAnyValue(x => x.ViewModel)
                    .Where(x => !x.IsLoggedInUser)
                    .Select(x => x.ShowMenuCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Action, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.Avatar)
                    .Subscribe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown)));

                d(this.WhenAnyValue(x => x.ViewModel.User.Name).Select(x => string.IsNullOrEmpty(x) ? null : x)
                    .Subscribe(x => RefreshHeaderView(subtext: x)));

                d(followers.BindText(this.WhenAnyValue(x => x.ViewModel.User.Followers)));
                d(following.BindText(this.WhenAnyValue(x => x.ViewModel.User.Following)));
                d(this.WhenAnyValue(x => x.ViewModel.HasBlog).Subscribe(x => website.Hidden = !x));
            });
        }
    }
}

