using CoreGraphics;
using Foundation;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers
{
    public class MessageComposerViewController : BaseViewController, IActivatable
    {
        private CGRect _keyboardBounds = CGRect.Empty;
        private NSObject _keyboardHideObserver;
        private NSObject _keyboardShowObserver;

        public ExtendedUITextView TextView { get; }

        public MessageComposerViewController()
        {
            EdgesForExtendedLayout = UIRectEdge.None;
            TextView = new ExtendedUITextView();
            TextView.Font = UIFont.PreferredBody;

            // Work around an Apple bug in the UITextView that crashes
            if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR)
                TextView.AutocorrectionType = UITextAutocorrectionType.No;

            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };
        }

        public string Text
        {
            get { return TextView.Text; }
            set
            {
                if (string.Equals(Text, value))
                    return;

                TextView.Text = value;
                TextView.SelectedRange = new NSRange(0, 0);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            ResizeTextView();
        }

        void KeyboardWillShow (NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            _keyboardBounds = nsValue.RectangleFValue;
            UIView.Animate(0.25f, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, ResizeTextView, null);
        }

        void KeyboardWillHide (NSNotification notification)
        {
            _keyboardBounds = CGRect.Empty;
            UIView.Animate(0.2, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, ResizeTextView, null);
        }

        private void ResizeTextView()
        {
            TextView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - _keyboardBounds.Height);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            _keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillHideNotification"), KeyboardWillHide);
            ResizeTextView();
            Add(TextView);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            TextView.ResignFirstResponder();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardHideObserver);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardShowObserver);
            TextView.RemoveFromSuperview();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            TextView.BecomeFirstResponder();
        }

        private static float CalculateHeight(UIInterfaceOrientation orientation)
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                return 44;
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
    }
}
