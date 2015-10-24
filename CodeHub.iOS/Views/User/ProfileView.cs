using Cirrious.MvvmCross.Binding.BindingContext;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.User;
using MonoTouch.Dialog;
using UIKit;

namespace CodeHub.iOS.Views.User
{
    public class ProfileView : ViewModelDrivenDialogViewController
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
            Title = "Profile";

            base.ViewDidLoad();

			var header = new HeaderView();
            var set = this.CreateBindingSet<ProfileView, ProfileViewModel>();
            set.Bind(header).For(x => x.Title).To(x => x.Username).OneWay();
			set.Bind(header).For(x => x.Subtitle).To(x => x.User.Name).OneWay();
			set.Bind(header).For(x => x.ImageUri).To(x => x.User.AvatarUrl).OneWay();
            set.Apply();

			var followers = new StyledStringElement("Followers".t(), () => ViewModel.GoToFollowersCommand.Execute(null), Images.Heart);
			var following = new StyledStringElement("Following".t(), () => ViewModel.GoToFollowingCommand.Execute(null), Images.Following);
			var events = new StyledStringElement("Events".t(), () => ViewModel.GoToEventsCommand.Execute(null), Images.Event);
			var organizations = new StyledStringElement("Organizations".t(), () => ViewModel.GoToOrganizationsCommand.Execute(null), Images.Group);
			var repos = new StyledStringElement("Repositories".t(), () => ViewModel.GoToRepositoriesCommand.Execute(null), Images.Repo);
			var gists = new StyledStringElement("Gists", () => ViewModel.GoToGistsCommand.Execute(null), Images.Script);

			Root.Add(new [] { new Section(header), new Section { events, organizations, followers, following }, new Section { repos, gists } });

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

