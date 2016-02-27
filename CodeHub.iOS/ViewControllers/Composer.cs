using System;
using CoreGraphics;
using CodeHub.iOS.Views;
using Foundation;
using UIKit;
using System.Collections.Generic;

namespace CodeHub.iOS.ViewControllers
{
    public class Composer : BaseViewController
    {
        protected UIBarButtonItem SendItem;
        UIViewController _previousController;
        public Action<string> ReturnAction;
        protected readonly UITextView TextView;
        protected UIView ScrollingToolbarView;
        private UIImage _normalButtonImage;
        private UIImage _pressedButtonImage;

        public bool EnableSendButton
        {
            get { return SendItem.Enabled; }
            set { SendItem.Enabled = value; }
        }

        public Composer () : base (null, null)
        {
            Title = "New Comment";
            EdgesForExtendedLayout = UIRectEdge.None;

            var close = new UIBarButtonItem { Image = Images.Buttons.CancelButton };
            NavigationItem.LeftBarButtonItem = close;
            SendItem = new UIBarButtonItem { Image = Images.Buttons.SaveButton };
            NavigationItem.RightBarButtonItem = SendItem;

            TextView = new UITextView(ComputeComposerSize(CGRect.Empty));
            TextView.Font = UIFont.PreferredBody;
            TextView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;

            // Work around an Apple bug in the UITextView that crashes
            if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR)
                TextView.AutocorrectionType = UITextAutocorrectionType.No;

            View.AddSubview (TextView);

            _normalButtonImage = ImageFromColor(UIColor.White);
            _pressedButtonImage = ImageFromColor(UIColor.FromWhiteAlpha(0.0f, 0.4f));

            OnActivation(d =>
            {
                d(close.GetClickedObservable().Subscribe(_ => CloseComposer()));
                d(SendItem.GetClickedObservable().Subscribe(_ => PostCallback()));
            });
        }

        private UIImage ImageFromColor(UIColor color)
        {
            UIGraphics.BeginImageContext(new CGSize(1, 1));
            var context = UIGraphics.GetCurrentContext();
            context.SetFillColor(color.CGColor);
            context.FillRect(new CGRect(0, 0, 1, 1));
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
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

        public void SetAccesoryButtons(IEnumerable<UIButton> buttons)
        {
            foreach (var button in buttons)
            {
                button.SetBackgroundImage(_normalButtonImage, UIControlState.Normal);
                button.SetBackgroundImage(_pressedButtonImage, UIControlState.Highlighted);
            }

            var height = CalculateHeight(UIApplication.SharedApplication.StatusBarOrientation);
            ScrollingToolbarView = new ScrollingToolbarView(new CGRect(0, 0, View.Bounds.Width, height), buttons);
            ScrollingToolbarView.BackgroundColor = UIColor.FromWhiteAlpha(0.7f, 1.0f);
            TextView.InputAccessoryView = ScrollingToolbarView;
        }

        public string Text
        {
            get { return TextView.Text; }
            set { TextView.Text = value; }
        }

        public void CloseComposer ()
        {
            SendItem.Enabled = true;
            _previousController.DismissViewController(true, null);
        }

        void PostCallback ()
        {
            SendItem.Enabled = false;
            TextView.ResignFirstResponder();

            try
            {
                if (ReturnAction != null)
                    ReturnAction(Text);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + " - " + e.StackTrace);
            }
        }
        
        void KeyboardWillShow (NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            var kbdBounds = nsValue.RectangleFValue;
            UIView.Animate(1.0f, 0, UIViewAnimationOptions.CurveEaseIn, () => TextView.Frame = ComputeComposerSize (kbdBounds), null);
        }

        void KeyboardWillHide (NSNotification notification)
        {
            TextView.Frame = ComputeComposerSize(new CGRect(0, 0, 0, 0));
        }

        CGRect ComputeComposerSize (CGRect kbdBounds)
        {
            var view = View.Bounds;
            return new CGRect (0, 0, view.Width, view.Height-kbdBounds.Height);
        }

        NSObject _hideNotification, _showNotification;
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            _hideNotification = NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillHideNotification"), KeyboardWillHide);
            TextView.BecomeFirstResponder ();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (_hideNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_hideNotification);
            if (_showNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }
        
        public void NewComment (UIViewController parent, Action<string> action)
        {
            Title = Title;
            ReturnAction = action;
            _previousController = parent;
            TextView.BecomeFirstResponder ();
            var nav = new UINavigationController(this);
            parent.PresentViewController(nav, true, null);
        }
    }
}
