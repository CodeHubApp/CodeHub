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
            this.WhenAnyValue(x => x.ViewModel.IsLoggedInUser)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x ? 
                    null : ViewModel.ShowMenuCommand.ToBarButtonItem(UIBarButtonSystemItem.Action));

            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);
            HeaderView.Image = Images.LoginUserUnknown;

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)))
                .Switch()
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsFollowing).Where(x => x.HasValue))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HeaderView.SetSubImage(x.Value ? Images.Star.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) : null));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-", () => ViewModel.GoToFollowersCommand.ExecuteIfCan());
            var following = split.AddButton("Following", "-", () => ViewModel.GoToFollowingCommand.ExecuteIfCan());
            var events = new DialogStringElement("Events", () => ViewModel.GoToEventsCommand.ExecuteIfCan(), Images.Rss);
            var organizations = new DialogStringElement("Organizations", () => ViewModel.GoToOrganizationsCommand.ExecuteIfCan(), Images.Organization);
            var repos = new DialogStringElement("Repositories", () => ViewModel.GoToRepositoriesCommand.ExecuteIfCan(), Images.Repo);
            var gists = new DialogStringElement("Gists", () => ViewModel.GoToGistsCommand.ExecuteIfCan(), Images.Gist);
            Root.Reset(new [] { new Section { split }, new Section { events, organizations, repos, gists } });

            this.WhenAnyValue(x => x.ViewModel.User).IsNotNull().Subscribe(x =>
            {
                HeaderView.ImageUri = x.AvatarUrl;
                followers.Text = x.Followers.ToString();
                following.Text = x.Following.ToString();
                HeaderView.SubText = string.IsNullOrEmpty(x.Name) ? null : x.Name;
                TableView.ReloadData();
            });
        }
    }
}

