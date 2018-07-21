using System;
using System.Collections.Generic;
using CodeHub.iOS.Views;
using CoreGraphics;
using Foundation;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class TextViewController : BaseViewController
    {
        public UITextView TextView { get; }

        public string Text
        {
            get => TextView.Text;
            set => TextView.Text = value;
        }

        public TextViewController()
        {
            EdgesForExtendedLayout = UIRectEdge.None;

            TextView = new UITextView(new CGRect(CGPoint.Empty, View.Bounds.Size))
            {
                Font = UIFont.PreferredBody,
                AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.AddSubview(TextView);
        }

        private float CalculateHeight(UIInterfaceOrientation orientation)
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                return 44;

            // If  pad
            if (orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown)
                return 64;
            return 88f;
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillRotate(toInterfaceOrientation, duration);

            if (TextView.InputAccessoryView != null)
            {
                UIView.Animate(duration, 0, UIViewAnimationOptions.BeginFromCurrentState, () =>
                {
                    var frame = TextView.InputAccessoryView.Frame;
                    frame.Height = CalculateHeight(toInterfaceOrientation);
                    TextView.InputAccessoryView.Frame = frame;
                }, null);
            }
        }

        void KeyboardChange(NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.FrameEndUserInfoKey) as NSValue;
            if (nsValue == null) return;

            var kbdBounds = nsValue.RectangleFValue;
            var keyboard = View.Window.ConvertRectToView(kbdBounds, View);

            UIView.Animate(
                1.0f, 0, UIViewAnimationOptions.CurveEaseIn,
                () => TextView.Frame = new CGRect(0, 0, View.Bounds.Width, keyboard.Top), null);
        }

        NSObject _hideNotification, _showNotification;
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("UIKeyboardWillShowNotification"), KeyboardChange);
            
            _hideNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("UIKeyboardWillHideNotification"), KeyboardChange);
            
            TextView.BecomeFirstResponder();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (_hideNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_hideNotification);
            if (_showNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }

        public static UIButton CreateAccessoryButton(UIImage image, Action action)
        {
            var btn = CreateAccessoryButton(string.Empty, action);
            //            btn.AutosizesSubviews = true;
            btn.SetImage(image, UIControlState.Normal);
            btn.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            btn.ImageEdgeInsets = new UIEdgeInsets(6, 6, 6, 6);

            //            var imageView = new UIImageView(image);
            //            imageView.Frame = new RectangleF(4, 4, 24, 24);
            //            imageView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            //            btn.Add(imageView);
            return btn;
        }

        public static UIButton CreateAccessoryButton(string title, Action action)
        {
            var fontSize = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 22 : 28f;

            var btn = new UIButton(UIButtonType.System);
            btn.Frame = new CGRect(0, 0, 32, 32);
            btn.SetTitle(title, UIControlState.Normal);
            btn.BackgroundColor = UIColor.White;
            btn.Font = UIFont.SystemFontOfSize(fontSize);
            btn.Layer.CornerRadius = 7f;
            btn.Layer.MasksToBounds = true;
            btn.AdjustsImageWhenHighlighted = false;
            btn.TouchUpInside += (sender, e) => action();
            return btn;
        }

        public void SetAccesoryButtons(IEnumerable<UIButton> buttons)
        {
            var normalButtonImage = new Lazy<UIImage>(
                () => Graphics.ImageFromColor(UIColor.White));
            var pressedButtonImage = new Lazy<UIImage>(
                () => Graphics.ImageFromColor(UIColor.FromWhiteAlpha(0.0f, 0.4f)));

            foreach (var button in buttons)
            {
                button.SetBackgroundImage(normalButtonImage.Value, UIControlState.Normal);
                button.SetBackgroundImage(pressedButtonImage.Value, UIControlState.Highlighted);
            }

            var height = CalculateHeight(UIApplication.SharedApplication.StatusBarOrientation);
            var scrollingToolbarView = new ScrollingToolbarView(new CGRect(0, 0, View.Bounds.Width, height), buttons);
            scrollingToolbarView.BackgroundColor = UIColor.FromWhiteAlpha(0.7f, 1.0f);
            TextView.InputAccessoryView = scrollingToolbarView;
        }
    }
}
