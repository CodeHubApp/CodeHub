using System;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.Images;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.App;
using SDWebImage;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.App
{
    public class StartupView : BaseViewController<StartupViewModel>
    {
        const float ImageSize = 128f;

        private readonly UIImageView _imgView = new UIImageView() {
            Alpha = 0,
            Image = Images.LoginUserUnknown,
        };

        private readonly UILabel _statusLabel = new UILabel {
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.FromName("HelveticaNeue", 13f),
            TextColor = UIColor.FromWhiteAlpha(0.34f, 1f),
            Alpha = 0,
        };

        private readonly UIActivityIndicatorView _activityView = new UIActivityIndicatorView {
            HidesWhenStopped = true,
            Color = UIColor.FromRGB(0.33f, 0.33f, 0.33f),
            Alpha = 0,
        };

        public StartupView()
        {
            Appearing.Subscribe(_ => UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade));
            Appearing.Where(_ => ViewModel != null).Subscribe(_ => ViewModel.StartupCommand.ExecuteIfCan());
            Disappearing.Subscribe(_ => {
                UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Fade);
                UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, true);
            });
        }
 
        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            _imgView.Frame = new System.Drawing.RectangleF(View.Bounds.Width / 2 - ImageSize / 2, View.Bounds.Height / 2 - ImageSize / 2 - 30f, ImageSize, ImageSize);
            _statusLabel.Frame = new System.Drawing.RectangleF(0, _imgView.Frame.Bottom + 10f, View.Bounds.Width, 15f);
            _activityView.Center = new System.Drawing.PointF(View.Bounds.Width / 2, _statusLabel.Frame.Bottom + 16f + 16F);

            try
            {
                View.BackgroundColor = BackgroundHelper.CreateRepeatingBackground();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to produce background color for StartupView: {0}", e.Message);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.AutosizesSubviews = true;

            _imgView.Layer.CornerRadius = ImageSize / 2;
            _imgView.Layer.MasksToBounds = true;
            Add(_imgView);

            Add(_statusLabel);
            Add(_activityView);

            this.WhenViewModel(x => x.IsLoggingIn).Subscribe(x =>
            {
                if (x)
                {
                    UIView.Animate(0.1, 0, UIViewAnimationOptions.TransitionCrossDissolve, () =>
                        _imgView.Alpha = _statusLabel.Alpha = _activityView.Alpha = 1, null);
                    _activityView.StartAnimating();
                }
                else
                {
                    UIView.Animate(0.1, 0, UIViewAnimationOptions.TransitionCrossDissolve, () =>
                        _imgView.Alpha = _statusLabel.Alpha = _activityView.Alpha = 0, null);
                    _activityView.StopAnimating();
                }
            });

            ViewModel.WhenAnyValue(x => x.Status).Subscribe(x => _statusLabel.Text = x);
            ViewModel.WhenAnyValue(x => x.ImageUrl).IsNotNull().Subscribe(x => {
                try
                {
                    _imgView.SetImage(new NSUrl(x.AbsoluteUri), Images.LoginUserUnknown, (img, err, type) =>
                        UIView.Transition(_imgView, 0.35f, UIViewAnimationOptions.TransitionCrossDissolve, () => _imgView.Image = img, null));
                }
                catch
                {
                    // Ah crap...
                }
            });

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

