using Cirrious.MvvmCross.Binding.BindingContext;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.User;
using MonoTouch.Dialog;
using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.Views.User
{
    public class ProfileView : PrettyDialogViewController
    {
		public new ProfileViewModel ViewModel
		{
			get { return (ProfileViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public ProfileView()
		{
			Root.UnevenRows = true;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = ViewModel.Username;
            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = ViewModel.Username;

            ViewModel.Bind(x => x.User, x =>
            {
                HeaderView.SubText = string.IsNullOrWhiteSpace(x.Name) ? null : x.Name;
                HeaderView.SetImage(x.AvatarUrl, Images.Avatar);
                RefreshHeaderView();
            });

			var followers = new StyledStringElement("Followers".t(), () => ViewModel.GoToFollowersCommand.Execute(null), Images.Heart);
			var following = new StyledStringElement("Following".t(), () => ViewModel.GoToFollowingCommand.Execute(null), Images.Following);
			var events = new StyledStringElement("Events".t(), () => ViewModel.GoToEventsCommand.Execute(null), Images.Event);
			var organizations = new StyledStringElement("Organizations".t(), () => ViewModel.GoToOrganizationsCommand.Execute(null), Images.Group);
			var repos = new StyledStringElement("Repositories".t(), () => ViewModel.GoToRepositoriesCommand.Execute(null), Images.Repo);
			var gists = new StyledStringElement("Gists", () => ViewModel.GoToGistsCommand.Execute(null), Images.Script);

            Root.Add(new [] { new Section(new UIView(new CGRect(0, 0, 0, 20f))) { events, organizations, followers, following }, new Section { repos, gists } });

			if (!ViewModel.IsLoggedInUser)
				NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
        }

		private void ShowExtraMenu()
		{
            var sheet = new UIActionSheet();
			var followButton = sheet.AddButton(ViewModel.IsFollowing ? "Unfollow".t() : "Follow".t());
			var cancelButton = sheet.AddButton("Cancel".t());
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);
			sheet.Dismissed += (s, e) => {
				if (e.ButtonIndex == followButton)
				{
					ViewModel.ToggleFollowingCommand.Execute(null);
				}

                sheet.Dispose();
			};

			sheet.ShowInView(this.View);
		}
    }
}

