using System;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.App;
using MonoTouch.SlideoutNavigation;
using CodeHub.iOS.ViewControllers;
using Akavache;
using Splat;

namespace CodeHub.iOS.ViewControllers.App
{
    public class StartupViewController : BaseViewController<StartupViewModel>
    {
        const float ImageSize = 128f;
        private UIImageView _imgView;
        private UILabel _statusLabel;
        private UIActivityIndicatorView _activityView;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            ViewModel.StartupCommand.ExecuteIfCan();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Fade);
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, true);
        }

        protected override void HandleNavigation(CodeHub.Core.ViewModels.IBaseViewModel viewModel, UIViewController view)
        {
//            if (view is MenuViewController)
//            {
//                var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
//                var nav = ((UINavigationController)appDelegate.Window.RootViewController);
//                var slideout = new SlideoutNavigationController();
//                slideout.MenuViewController = new MenuNavigationController(view, slideout);
//                UIView.Transition(nav.View, 0.3, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.TransitionCrossDissolve,
//                    () => nav.PushViewController(slideout, false), null);
//            }
//            else
//            {
//                PresentViewController(new ThemedNavigationController(view), true, null);
//                viewModel.RequestDismiss.Subscribe(_ => DismissViewController(true, null));
//            }

            var nav = new ThemedNavigationController(view);
            PresentViewController(nav, true, null);
//            NavigationController.PushViewController(nav, true);
        }
 
        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            _imgView.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2 - ImageSize / 2, View.Bounds.Height / 2 - ImageSize / 2 - 30f, ImageSize, ImageSize);
            _statusLabel.Frame = new CoreGraphics.CGRect(0, _imgView.Frame.Bottom + 10f, View.Bounds.Width, 15f);
            _activityView.Center = new CoreGraphics.CGPoint(View.Bounds.Width / 2, _statusLabel.Frame.Bottom + 16f + 16F);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.AutosizesSubviews = true;
            View.BackgroundColor = UIColor.FromRGB (221, 221, 221);

            _activityView = new UIActivityIndicatorView {
                HidesWhenStopped = true,
                Color = UIColor.FromRGB(0.33f, 0.33f, 0.33f),
                Alpha = 0,
            };
            Add(_activityView);

            _statusLabel = new UILabel {
                TextAlignment = UITextAlignment.Center,
                Font = UIFont.FromName("HelveticaNeue", 13f),
                TextColor = UIColor.FromWhiteAlpha(0.34f, 1f),
                Alpha = 0,
            };
            Add(_statusLabel);

            _imgView = new UIImageView {
                Alpha = 0,
                Image = Images.LoginUserUnknown,
            };
            _imgView.Layer.CornerRadius = ImageSize / 2;
            _imgView.Layer.MasksToBounds = true;
            Add(_imgView);

//            this.WhenAnyValue(x => x.ViewModel.IsLoggingIn).Where(x => x).Subscribe(x => {
//                UIView.Animate(0.3, 0, UIViewAnimationOptions.TransitionCrossDissolve | UIViewAnimationOptions.BeginFromCurrentState, () =>
//                    _imgView.Alpha = _statusLabel.Alpha = _activityView.Alpha = 1, null);
//                _activityView.StartAnimating();
//            });
//
//            this.WhenAnyValue(x => x.ViewModel.IsLoggingIn).Where(x => !x).Subscribe(x => {
//                UIView.Animate(0.3, 0, UIViewAnimationOptions.TransitionCrossDissolve | UIViewAnimationOptions.BeginFromCurrentState, () =>
//                    _imgView.Alpha = _statusLabel.Alpha = _activityView.Alpha = 0, null);
//                _activityView.StopAnimating();
//            });
//
//            this.WhenAnyValue(x => x.ViewModel.Status).Subscribe(x => _statusLabel.Text = x);
//
//            this.WhenAnyValue(x => x.ViewModel.Avatar)
//                .Select(x => x.ToUri(128))
//                .Where(x => x != null)
//                .Select(x => BlobCache.LocalMachine.LoadImageFromUrl(x.AbsoluteUri))
//                .Switch()
//                .ObserveOn(RxApp.MainThreadScheduler)
//                .Subscribe(img => {
//                    _imgView.Image = Images.LoginUserUnknown;
//                    UIView.Transition(_imgView, 0.35f, UIViewAnimationOptions.TransitionCrossDissolve | UIViewAnimationOptions.BeginFromCurrentState, () => _imgView.Image = img.ToNative(), null);
//                });
        }

        public override bool ShouldAutorotate()
        {
            return true;
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

        /// <summary>
        /// A custom navigation controller specifically for iOS6 that locks the orientations to what the StartupControler's is.
        /// </summary>
        protected class CustomNavigationController : UINavigationController
        {
            readonly StartupViewController _parent;
            public CustomNavigationController(StartupViewController parent, UIViewController root) : base(root) 
            { 
                _parent = parent;
            }

            public override bool ShouldAutorotate()
            {
                return _parent.ShouldAutorotate();
            }

            public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
            {
                return _parent.GetSupportedInterfaceOrientations();
            }
        }
    }
}

