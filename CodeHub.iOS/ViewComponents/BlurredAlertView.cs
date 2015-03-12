using System;
using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.ViewComponents
{
    public class BlurredAlertView : UIView
    {
        private readonly UILabel _label;
        private readonly UIButton _button;

        public static UIView Display(string text, Action dismissed)
        {
            var window = ((AppDelegate)UIApplication.SharedApplication.Delegate).Window;

            var blur = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
            var blurView = new UIVisualEffectView(blur);
            blurView.Frame = new CGRect(0, 0, window.Frame.Width, window.Frame.Height);

            var alertView = new BlurredAlertView(text);
            alertView.Frame = new CGRect(0, 0, blurView.Frame.Width, blurView.Frame.Height);

            blurView.ContentView.Add(alertView);
            blurView.Alpha = 0;
            window.Add(blurView);

            UIView.Animate(0.4, 0, UIViewAnimationOptions.CurveEaseIn, () => blurView.Alpha = 1, null);

            alertView._button.TouchUpInside += (sender, e) =>
                UIView.Animate(0.4, 0, UIViewAnimationOptions.CurveEaseIn, () => blurView.Alpha = 0, () => {
                    blurView.RemoveFromSuperview();
                    dismissed();
                });

            return blurView;
        }

        private BlurredAlertView(string text)
            : base(new CGRect(0, 0, 320f, 480f))
        {
            _label = new UILabel();
            _label.Lines = 0;
            _label.Text = text;
            _label.Font = UIFont.PreferredHeadline;
            _label.TextAlignment = UITextAlignment.Center;
            _label.TextColor = UIColor.White;

            _button = new UIButton(UIButtonType.Custom);
            _button.Frame = new CGRect(0, 0, 100f, 44f);

            var buttonLabel = new UILabel();
            buttonLabel.Text = "Ok";
            buttonLabel.Font = UIFont.PreferredBody.MakeBold();
            buttonLabel.TextColor = UIColor.White;
            buttonLabel.SizeToFit();
            buttonLabel.Center = new CGPoint(_button.Frame.Width / 2, _button.Frame.Height / 2);
            buttonLabel.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

            _button.Add(buttonLabel);

            _button.Layer.BorderColor = UIColor.White.CGColor;
            _button.Layer.BorderWidth = 1f;
            _button.Layer.CornerRadius = 6f;
            _button.Layer.MasksToBounds = true;

            Add(_label);
            Add(_button);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _label.Frame = new CGRect(0, 0, 280f, Bounds.Height);
            _label.SizeToFit();

            _label.Center = new CGPoint(Bounds.Width / 2, Bounds.Height / 2 - _label.Frame.Height / 4);

            _button.Frame = new CGRect(0, 0, 200f, 44f);
            _button.Center = new CGPoint(Bounds.Width / 2f, _label.Frame.Bottom + _button.Frame.Height + 20f);
        }
    }
}

