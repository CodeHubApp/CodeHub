using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.DialogElements;
using CodeHub.iOS.Elements;

namespace CodeHub.iOS.Views.Users
{
    public class UserView : ReactiveDialogViewController<UserViewModel>
    {
        public UserView()
        {
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

