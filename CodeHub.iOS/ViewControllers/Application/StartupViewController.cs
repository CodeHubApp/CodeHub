using System;
using UIKit;
using CodeHub.iOS;
using SDWebImage;
using Foundation;
using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.ViewControllers;
using MvvmCross.Platform;
using CodeHub.Core.Factories;
using CodeHub.Core.Services;
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

        public StartupViewModel ViewModel { get; }

        public StartupViewController()
        {
            ViewModel = new StartupViewModel(
                Mvx.Resolve<ILoginFactory>(),
                Mvx.Resolve<IApplicationService>(),
                Mvx.Resolve<IDefaultValueService>());
        }

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
                d(ViewModel.GoToMenu.Subscribe(GoToMenu));
                d(ViewModel.GoToAccounts.Subscribe(GoToAccounts));
                d(ViewModel.GoToNewAccount.Subscribe(GoToNewAccount));
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

        private void GoToMenu(object o)
        {
            var vc = new MenuViewController();
            var slideoutController = new SlideoutNavigationController();
            slideoutController.MenuViewController = new MenuNavigationController(vc, slideoutController);
            (UIApplication.SharedApplication.Delegate as AppDelegate).Do(y => y.Presenter.SlideoutNavigationController = slideoutController);
            vc.ViewModel.GoToDefaultTopView.Execute(null);
            slideoutController.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
            PresentViewController(slideoutController, true, null);
        }

        private void GoToNewAccount(object o)
        {
            var vc = new NewAccountViewController();
            var nav = new ThemedNavigationController(vc);
            PresentViewController(nav, true, null);
        }

        private void GoToAccounts(object o)
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
                    UIView.Transition(_imgView, 0.25f, UIViewAnimationOptions.TransitionCrossDissolve, () => _imgView.Image = img, null);
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

