using System;
using System.Reactive.Linq;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Users;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeStash.iOS.Views;
using CodeFramework.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Users
{
    public class ProfileView : ViewModelDialogView<ProfileViewModel>
    {
        private UIActionSheet _actionSheet;
        private SlideUpTitleView _slideUpTitle;

		public ProfileView()
		{
			Root.UnevenRows = true;
		}

        protected override void Scrolled(System.Drawing.PointF point)
        {
            if (point.Y > 0)
            {
                NavigationController.NavigationBar.ShadowImage = null;
            }
            else
            {
                if (NavigationController.NavigationBar.ShadowImage == null)
                    NavigationController.NavigationBar.ShadowImage = new UIImage();
            }

            _slideUpTitle.Offset = 108 + 28f - point.Y;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.ShadowImage = new UIImage();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.NavigationBar.ShadowImage = null;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.TitleView = _slideUpTitle = new SlideUpTitleView(NavigationController.NavigationBar.Bounds.Height);
            _slideUpTitle.Text = ViewModel.Username;
            _slideUpTitle.Offset = 100f;

            TableView.SectionHeaderHeight = 0;
            RefreshControl.TintColor = UIColor.LightGray;

            var header = new ImageAndTitleHeaderView 
            { 
                Text = ViewModel.Username,
                BackgroundColor = NavigationController.NavigationBar.BackgroundColor,
                TextColor = UIColor.White,
                SubTextColor = UIColor.LightGray
            };

            var topBackgroundView = this.CreateTopBackground(header.BackgroundColor);
            topBackgroundView.Hidden = true;

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-", () => ViewModel.GoToFollowersCommand.ExecuteIfCan());
            var following = split.AddButton("Following", "-", () => ViewModel.GoToFollowingCommand.ExecuteIfCan());
			var events = new StyledStringElement("Events", () => ViewModel.GoToEventsCommand.ExecuteIfCan(), Images.Event);
			var organizations = new StyledStringElement("Organizations", () => ViewModel.GoToOrganizationsCommand.ExecuteIfCan(), Images.Group);
			var repos = new StyledStringElement("Repositories", () => ViewModel.GoToRepositoriesCommand.ExecuteIfCan(), Images.Repo);
			var gists = new StyledStringElement("Gists", () => ViewModel.GoToGistsCommand.ExecuteIfCan(), Images.Script);

            Root.Add(new [] { new Section(header) { split }, new Section { events, organizations, repos, gists } });

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
                topBackgroundView.Hidden = false;
                //header.Subtitle = x.Name;
                header.ImageUri = x.AvatarUrl;
                followers.Text = x.Followers.ToString();
                following.Text = x.Following.ToString();

                if (!string.IsNullOrEmpty(x.Name))
                    header.SubText = x.Name;
                ReloadData();
            });

        }
    }
}

