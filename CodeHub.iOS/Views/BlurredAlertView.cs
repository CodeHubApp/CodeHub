using System;
using UIKit;
using CoreGraphics;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views
{
    public class BlurredAlertView : UIViewController
    {
        private readonly UILabel _label;
        private readonly UIButton _button;

        public static void Display(string text, Action dismissed = null)
        {
            var window = ((AppDelegate)UIApplication.SharedApplication.Delegate).Window;

            var alertView = new BlurredAlertView(text);
            var blur = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
            var blurView = new UIVisualEffectView(blur);
            blurView.Frame = new CGRect(0, 0, window.Frame.Width, window.Frame.Height);
            blurView.AutoresizingMask = UIViewAutoresizing.All;

            blurView.ContentView.Add(alertView.View);
            blurView.Alpha = 0;
            blurView.AutoresizingMask = UIViewAutoresizing.All;
            window.Add(blurView);

            UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseIn, () => blurView.Alpha = 1, null);

            alertView._button.GetClickedObservable().Take(1).Subscribe(_ => {
                UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseIn, () => blurView.Alpha = 0, () => {
                    blurView.RemoveFromSuperview();
                    alertView.View.RemoveFromSuperview();
                    dismissed?.Invoke();
                });
            });
        }

        private BlurredAlertView(string text)
        {
            _label = new UILabel();
            _label.Lines = 0;
            _label.Text = text;
            _label.Font = UIFont.PreferredHeadline;
            _label.TextAlignment = UITextAlignment.Center;
            _label.TextColor = UIColor.White;

            _button = new UIButton(UIButtonType.Custom);

            var buttonLabel = new UILabel();
            buttonLabel.Text = "Ok";
            buttonLabel.Font = UIFont.PreferredBody.MakeBold();
            buttonLabel.TextColor = UIColor.White;
            buttonLabel.SizeToFit();
            buttonLabel.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

            _button.Add(buttonLabel);

            _button.Layer.BorderColor = UIColor.White.CGColor;
            _button.Layer.BorderWidth = 1f;
            _button.Layer.CornerRadius = 6f;
            _button.Layer.MasksToBounds = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.Clear;

            Add(_label);
            Add(_button);

            View.ConstrainLayout(() => 
                _label.Frame.Width <= 360f &&
                _label.Frame.Left >= View.Frame.Left + 20f &&
                _label.Frame.Right <= View.Frame.Right - 20f &&
                _label.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                _label.Frame.GetCenterY() == View.Frame.GetCenterY() - 30f &&

                _button.Frame.Width == 140f &&
                _button.Frame.Height == 44f &&
                _button.Frame.GetCenterX() == View.Frame.GetCenterX() &&
                _button.Frame.Top == _label.Frame.Bottom + 30f
            );
        }
    }
}

