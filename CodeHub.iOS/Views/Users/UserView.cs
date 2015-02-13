using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Users
{
    public class UserView : BaseDialogViewController<UserViewModel>
    {
        public UserView()
        {
            HeaderView.Image = Images.LoginUserUnknown;

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)))
                .Switch()
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsFollowing).Where(x => x.HasValue))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HeaderView.SetSubImage(x.Value ? Octicon.Star.ToImage() : null));

            this.WhenAnyValue(x => x.ViewModel.IsLoggedInUser)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x ?
                    null : ViewModel.ShowMenuCommand.ToBarButtonItem(UIBarButtonSystemItem.Action));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-", () => ViewModel.GoToFollowersCommand.ExecuteIfCan());
            var following = split.AddButton("Following", "-", () => ViewModel.GoToFollowingCommand.ExecuteIfCan());
            var events = new StringElement("Events", () => ViewModel.GoToEventsCommand.ExecuteIfCan(), Octicon.Rss.ToImage());
            var organizations = new StringElement("Organizations", () => ViewModel.GoToOrganizationsCommand.ExecuteIfCan(), Octicon.Organization.ToImage());
            var repos = new StringElement("Repositories", () => ViewModel.GoToRepositoriesCommand.ExecuteIfCan(), Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", () => ViewModel.GoToGistsCommand.ExecuteIfCan(), Octicon.Gist.ToImage());
            Root.Reset(new [] { new Section { split }, new Section { events, organizations, repos, gists } });

            this.WhenAnyValue(x => x.ViewModel.User).IsNotNull().Subscribe(x =>
            {
                HeaderView.ImageUri = x.AvatarUrl;
                followers.Text = x.Followers.ToString();
                following.Text = x.Following.ToString();
                HeaderView.SubText = string.IsNullOrEmpty(x.Name) ? null : x.Name;
                RefreshHeaderView();
            });
        }
    }
}

