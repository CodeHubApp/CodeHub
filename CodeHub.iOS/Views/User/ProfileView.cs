using System;
using System.Reactive.Linq;
using CodeFramework.iOS.ViewComponents;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.User;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.User
{
    public class ProfileView : ViewModelDialogView<ProfileViewModel>
    {
        private UIActionSheet _actionSheet;

		public ProfileView()
		{
			Root.UnevenRows = true;
		}

        public override void ViewDidLoad()
        {
            Title = "Profile";

            base.ViewDidLoad();

            var header = new HeaderView {Title = ViewModel.Username};
            ViewModel.WhenAnyValue(x => x.User).Where(x => x != null).Subscribe(x =>
            {
                header.Subtitle = x.Name;
                header.ImageUri = x.AvatarUrl;
            });

			var followers = new StyledStringElement("Followers", () => ViewModel.GoToFollowersCommand.Execute(null), Images.Heart);
			var following = new StyledStringElement("Following", () => ViewModel.GoToFollowingCommand.Execute(null), Images.Following);
			var events = new StyledStringElement("Events", () => ViewModel.GoToEventsCommand.Execute(null), Images.Event);
			var organizations = new StyledStringElement("Organizations", () => ViewModel.GoToOrganizationsCommand.Execute(null), Images.Group);
			var repos = new StyledStringElement("Repositories", () => ViewModel.GoToRepositoriesCommand.Execute(null), Images.Repo);
			var gists = new StyledStringElement("Gists", () => ViewModel.GoToGistsCommand.Execute(null), Images.Script);

			Root.Add(new [] { new Section(header), new Section { events, organizations, followers, following }, new Section { repos, gists } });

			if (!ViewModel.IsLoggedInUser)
            {
				NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
                NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.IsFollowing, x => x != null));
            }
        }

		private void ShowExtraMenu()
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
					ViewModel.ToggleFollowingCommand.Execute(null);
			};

            _actionSheet.ShowInView(View);
		}
    }
}

