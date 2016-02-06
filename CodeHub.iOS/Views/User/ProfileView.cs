using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;
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
            
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = ViewModel.Username;
            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = ViewModel.Username;

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-", () => ViewModel.GoToFollowersCommand.Execute(null));
            var following = split.AddButton("Following", "-", () => ViewModel.GoToFollowingCommand.Execute(null));

            var events = new StyledStringElement("Events".t(), () => ViewModel.GoToEventsCommand.Execute(null), Octicon.Rss.ToImage());
            var organizations = new StyledStringElement("Organizations".t(), () => ViewModel.GoToOrganizationsCommand.Execute(null), Octicon.Organization.ToImage());
            var repos = new StyledStringElement("Repositories".t(), () => ViewModel.GoToRepositoriesCommand.Execute(null), Octicon.Repo.ToImage());
            var gists = new StyledStringElement("Gists", () => ViewModel.GoToGistsCommand.Execute(null), Octicon.Gist.ToImage());
            Root.UnevenRows = true;
            Root.Add(new [] { new Section { split }, new Section { events, organizations, repos, gists } });

            ViewModel.Bind(x => x.User, x =>
            {
                followers.Text = x.Followers.ToString();
                following.Text = x.Following.ToString();
                HeaderView.SubText = string.IsNullOrWhiteSpace(x.Name) ? null : x.Name;
                HeaderView.SetImage(x.AvatarUrl, Images.Avatar);
                RefreshHeaderView();
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (!ViewModel.IsLoggedInUser)
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

		private void ShowExtraMenu()
		{
            var sheet = new UIActionSheet();
			var followButton = sheet.AddButton(ViewModel.IsFollowing ? "Unfollow" : "Follow");
			var cancelButton = sheet.AddButton("Cancel");
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

