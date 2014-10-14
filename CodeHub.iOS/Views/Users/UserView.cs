using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Users
{
    public class UserView : ViewModelPrettyDialogViewController<UserViewModel>
    {
        private UIActionSheet _actionSheet;

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

            Root.Reset(new [] { new Section(HeaderView) { split }, new Section { events, organizations, repos, gists } });

			if (!ViewModel.IsLoggedInUser)
            {
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s_, e_) => 
                {
                    _actionSheet = new UIActionSheet(ViewModel.Username);
                    var followButton = ViewModel.IsFollowing.HasValue
                        ? _actionSheet.AddButton(ViewModel.IsFollowing.Value ? "Unfollow" : "Follow")
                        : -1;
                    var cancelButton = _actionSheet.AddButton("Cancel");
                    _actionSheet.CancelButtonIndex = cancelButton;
                    _actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
                    _actionSheet.Clicked += (s, e) => 
                    {
                        if (e.ButtonIndex == followButton)
                            ViewModel.ToggleFollowingCommand.ExecuteIfCan();
                        _actionSheet = null;
                    };

                    _actionSheet.ShowInView(View);
                });
                NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.IsFollowing, x => x.HasValue));
            }

            ViewModel.WhenAnyValue(x => x.User).Where(x => x != null).Subscribe(x =>
            {
                HeaderView.SubText = x.Name;
                HeaderView.ImageUri = x.AvatarUrl;
                followers.Text = x.Followers.ToString();
                following.Text = x.Following.ToString();

                if (!string.IsNullOrEmpty(x.Name))
                    HeaderView.SubText = x.Name;
                ReloadData();
            });

        }
    }
}

