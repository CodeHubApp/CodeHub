using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.DialogElements;

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
			var events = new StyledStringElement("Events", () => ViewModel.GoToEventsCommand.ExecuteIfCan(), Images.Event);
			var organizations = new StyledStringElement("Organizations", () => ViewModel.GoToOrganizationsCommand.ExecuteIfCan(), Images.Group);
			var repos = new StyledStringElement("Repositories", () => ViewModel.GoToRepositoriesCommand.ExecuteIfCan(), Images.Repo);
			var gists = new StyledStringElement("Gists", () => ViewModel.GoToGistsCommand.ExecuteIfCan(), Images.Script);
            Root.Reset(new [] { new Section { split }, new Section { events, organizations, repos, gists } });

            ViewModel.WhenAnyValue(x => x.User).IsNotNull().Subscribe(x =>
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

