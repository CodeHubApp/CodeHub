using System;
using UIKit;
using SDWebImage;
using Foundation;
using CodeHub.Core.ViewModels.App;
using MonoTouch.SlideoutNavigation;
using CodeHub.iOS.ViewControllers.Accounts;
using CodeHub.Core.Utilities;
using System.Linq;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class StartupViewController : BaseViewController
    {
        const float imageSize = 128f;

        private UIImageView _imgView;
        private UILabel _statusLabel;
        private UIActivityIndicatorView _activityView;

        public StartupViewModel ViewModel { get; } = new StartupViewModel();

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            _imgView.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2 - imageSize / 2, View.Bounds.Height / 2 - imageSize / 2 - 30f, imageSize, imageSize);
            _statusLabel.Frame = new CoreGraphics.CGRect(0, _imgView.Frame.Bottom + 10f, View.Bounds.Width, 20f);
            _activityView.Center = new CoreGraphics.CGPoint(View.Bounds.Width / 2, _statusLabel.Frame.Bottom + 16f + 16F);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.AutosizesSubviews = true;

            _imgView = new UIImageView();
            _imgView.TintColor = Theme.CurrentTheme.PrimaryColor;
            _imgView.Layer.CornerRadius = imageSize / 2;
            _imgView.Layer.MasksToBounds = true;
            Add(_imgView);

            _statusLabel = new UILabel();
            _statusLabel.TextAlignment = UITextAlignment.Center;
            _statusLabel.Font = UIFont.FromName("HelveticaNeue", 13f);
            _statusLabel.TextColor = UIColor.FromWhiteAlpha(0.34f, 1f);
            Add(_statusLabel);

            _activityView = new UIActivityIndicatorView();
            _activityView.Color = UIColor.FromRGB(0.33f, 0.33f, 0.33f);
            Add(_activityView);

            View.BackgroundColor = UIColor.FromRGB (221, 221, 221);

            OnActivation(d =>
            {
                d(ViewModel.Bind(x => x.ImageUrl).Subscribe(UpdatedImage));
                d(ViewModel.Bind(x => x.Status).Subscribe(x => _statusLabel.Text = x));
                d(ViewModel.GoToMenu.Subscribe(_ => GoToMenu()));
                d(ViewModel.GoToAccounts.Subscribe(_ => GoToAccounts()));
                d(ViewModel.GoToNewAccount.Subscribe(_ => GoToNewAccount()));
                d(ViewModel.Bind(x => x.IsLoggingIn).Subscribe(x =>
                {
                    if (x)
                        _activityView.StartAnimating();
                    else
                        _activityView.StopAnimating();

                    _activityView.Hidden = !x;
                }));
            });
        }

        private void GoToMenu()
        {
            var vc = new MenuViewController();
            var slideoutController = new SlideoutNavigationController();
            slideoutController.MenuViewController = new MenuNavigationController(vc, slideoutController);

            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            if (appDelegate != null)
                appDelegate.Presenter.SlideoutNavigationController = slideoutController;
            
            var openButton = new UIBarButtonItem { Image = Images.Buttons.ThreeLinesButton };
            var mainNavigationController = new MainNavigationController(GetInitialMenuViewController(), slideoutController, openButton);
            slideoutController.SetMainViewController(mainNavigationController, false);

            slideoutController.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
            AppDelegate.Instance.TransitionToViewController(slideoutController);
        }

        private UIViewController GetInitialMenuViewController()
        {
            var username = ViewModel.Account.Username;
            switch (ViewModel.Account.DefaultStartupView)
            {
                case "Organizations":
                    return new Organizations.OrganizationsViewController(username);
                case "Trending Repositories":
                    return new Repositories.TrendingRepositoriesViewController();
                case "Explore Repositories":
                    return new Search.ExploreViewController();
                case "Owned Repositories":
                    return Repositories.RepositoriesViewController.CreateMineViewController();
                case "Starred Repositories":
                    return Repositories.RepositoriesViewController.CreateStarredViewController();
                case "Public Gists":
                    return Gists.GistsViewController.CreatePublicGistsViewController();
                case "Starred Gists":
                    return Gists.GistsViewController.CreateStarredGistsViewController();
                case "My Gists":
                    return Gists.GistsViewController.CreateUserGistsViewController(username);
                case "Profile":
                    return new Users.UserViewController(username);
                case "My Events":
                    return new Events.UserEventsViewController(username);
                case "My Issues":
                    return Views.Issues.MyIssuesView.Create();
                case "Notifications":
                    return Views.NotificationsView.Create();
                default:
                    return Events.NewsViewController.Create();
                    
            }
        }

        private void GoToNewAccount()
        {
            var vc = new NewAccountViewController();
            var nav = new ThemedNavigationController(vc);
            PresentViewController(nav, true, null);
        }

        private void GoToAccounts()
        {
            var vc = new AccountsViewController();
            var nav = new ThemedNavigationController(vc);
            PresentViewController(nav, true, null);
        }

        public void UpdatedImage(Uri uri)
        {
            if (uri == null)
                AssignUnknownUserImage();
            else
            {
                // Wipe out old avatars
                var avatar = new GitHubAvatar(uri);
                var avatarSizes = new [] { avatar.ToUri(), avatar.ToUri(64) };
                foreach (var avatarUrl in avatarSizes.Select(x => new NSUrl(x.AbsoluteUri)) )
                {
                    var cacheKey = SDWebImageManager.SharedManager.CacheKey(avatarUrl);
                    if (cacheKey != null)
                        SDWebImageManager.SharedManager.ImageCache.RemoveImage(cacheKey);
                }

                _imgView.SetImage(new NSUrl(uri.AbsoluteUri), Images.LoginUserUnknown, (img, err, cache, _) => {
                    _imgView.Image = Images.LoginUserUnknown;
                    UIView.Transition(_imgView, 0.25f, UIViewAnimationOptions.TransitionCrossDissolve , () => _imgView.Image = img, null);
                });
            }
        }

        private void AssignUnknownUserImage()
        {
            var img = Images.LoginUserUnknown;
            _imgView.Image = img;
            _imgView.TintColor = UIColor.FromWhiteAlpha(0.34f, 1f);
        }


        public override void ViewWillAppear(bool animated)
        {
            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
            AssignUnknownUserImage();
            _statusLabel.Text = "";

            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Fade);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            ViewModel.StartupCommand.Execute(null);
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _imgView.Image = null;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.Default;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
            return UIInterfaceOrientationMask.All;
        }

        public override void PresentViewController(UIViewController viewControllerToPresent, bool animated, Action completionHandler)
        {
            if (PresentedViewController != null)
            {
                PresentedViewController.PresentViewController(viewControllerToPresent, animated, completionHandler);
            }
            else
            {
                base.PresentViewController(viewControllerToPresent, animated, completionHandler);
            }
        }
    }
}

