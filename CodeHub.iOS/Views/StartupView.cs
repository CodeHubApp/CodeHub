using System;
using MonoTouch;
using UIKit;
using CodeFramework.Core.ViewModels;
using CodeFramework.iOS.ViewControllers;
using MonoTouch.Dialog.Utilities;

namespace CodeFramework.iOS.Views
{
    public class StartupView : ViewModelDrivenDialogViewController, IImageUpdated
    {
        const float imageSize = 128f;

        private UIImageView _imgView;
        private UILabel _statusLabel;
        private UIActivityIndicatorView _activityView;
        private UIStatusBarStyle _previousStatusbarStyle;

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            _imgView.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2 - imageSize / 2, View.Bounds.Height / 2 - imageSize / 2 - 30f, imageSize, imageSize);
            _statusLabel.Frame = new CoreGraphics.CGRect(0, _imgView.Frame.Bottom + 10f, View.Bounds.Width, 15f);
            _activityView.Center = new CoreGraphics.CGPoint(View.Bounds.Width / 2, _statusLabel.Frame.Bottom + 16f + 16F);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.AutosizesSubviews = true;

            _imgView = new UIImageView();
            _imgView.Layer.CornerRadius = imageSize / 2;
            _imgView.Layer.MasksToBounds = true;
            Add(_imgView);

            _statusLabel = new UILabel();
            _statusLabel.TextAlignment = UITextAlignment.Center;
            _statusLabel.Font = UIFont.FromName("HelveticaNeue", 13f);
            _statusLabel.TextColor = UIColor.FromWhiteAlpha(0.34f, 1f);
            Add(_statusLabel);

            _activityView = new UIActivityIndicatorView() { HidesWhenStopped = true };
            _activityView.Color = UIColor.FromRGB(0.33f, 0.33f, 0.33f);
            Add(_activityView);

			View.BackgroundColor = UIColor.FromRGB (221, 221, 221);
           
			var vm = (BaseStartupViewModel)ViewModel;
			vm.Bind(x => x.IsLoggingIn, x =>
			{
				if (x)
				{
                    _activityView.StartAnimating();
				}
				else
				{
                    _activityView.StopAnimating();
				}
			});

            vm.Bind(x => x.ImageUrl, UpdatedImage);
            vm.Bind(x => x.Status, x => _statusLabel.Text = x);

        }

        public void UpdatedImage(Uri uri)
        {
            if (uri == null)
            {
                AssignUnknownUserImage();
            }
            else
            {
                var img = ImageLoader.DefaultRequestImage(uri, this);
                if (img == null)
                {
                    AssignUnknownUserImage();
                }
                else
                {
                    UIView.Transition(_imgView, 0.50f, UIViewAnimationOptions.TransitionCrossDissolve, () => _imgView.Image = img, null);
                }
            }
        }

        private void AssignUnknownUserImage()
        {
            var img = Theme.CurrentTheme.LoginUserUnknown.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            _imgView.Image = img;
            _imgView.TintColor = UIColor.FromWhiteAlpha(0.34f, 1f);
        }


        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _previousStatusbarStyle = UIApplication.SharedApplication.StatusBarStyle;
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.Default, false);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            UIApplication.SharedApplication.SetStatusBarStyle(_previousStatusbarStyle, true);
        }

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			var vm = (BaseStartupViewModel)ViewModel;
			vm.StartupCommand.Execute(null);
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
            readonly StartupView _parent;
            public CustomNavigationController(StartupView parent, UIViewController root) : base(root) 
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

