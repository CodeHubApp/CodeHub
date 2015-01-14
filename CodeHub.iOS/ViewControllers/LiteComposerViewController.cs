using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class LiteComposer : UIViewController
    {
        readonly ComposerView _composerView;
        internal UIBarButtonItem SendItem;

        public event EventHandler<string> ReturnAction;

        public bool EnableSendButton
        {
            get { return SendItem.Enabled; }
            set { SendItem.Enabled = value; }
        }

        private class ComposerView : UIView
        {
            internal readonly UITextView TextView;

            public ComposerView(RectangleF bounds)
                : base(bounds)
            {
                TextView = new UITextView(RectangleF.Empty)
                {
                    Font = UIFont.SystemFontOfSize(18),
                };

                // Work around an Apple bug in the UITextView that crashes
                if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
                    TextView.AutocorrectionType = UITextAutocorrectionType.No;

                AddSubview(TextView);
            }


            internal void Reset(string text)
            {
                TextView.Text = text;
            }

            public override void LayoutSubviews()
            {
                Resize(Bounds);
            }

            void Resize(RectangleF bounds)
            {
                TextView.Frame = new RectangleF(0, 0, bounds.Width, bounds.Height);
            }

            public string Text
            {
                get
                {
                    return TextView.Text;
                }
                set
                {
                    TextView.Text = value;
                }
            }
        }

        public LiteComposer()
            : base(null, null)
        {
            Title = "New Comment";
            EdgesForExtendedLayout = UIRectEdge.None;

            // Composer
            _composerView = new ComposerView(ComputeComposerSize(RectangleF.Empty));

            View.AddSubview(_composerView);
        }

        public string Text
        {
            get { return _composerView.Text; }
            set { _composerView.Text = value; }
        }

        public string ActionButtonText
        {
            get { return NavigationItem.RightBarButtonItem.Title; }
            set { NavigationItem.RightBarButtonItem.Title = value; }
        }

        public void CloseComposer()
        {
            SendItem.Enabled = true;
            NavigationController.PopViewControllerAnimated(true);
        }

        public void Save()
        {
            SendItem.Enabled = false;
            var handler = ReturnAction;
            if (handler != null)
                handler(this, Text);
        }

        void KeyboardWillShow(NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey(UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            var kbdBounds = nsValue.RectangleFValue;
            _composerView.Frame = ComputeComposerSize(kbdBounds);
        }

        RectangleF ComputeComposerSize(RectangleF kbdBounds)
        {
            var view = View.Bounds;
            return new RectangleF(0, 0, view.Width, view.Height - kbdBounds.Height);
        }

        [Obsolete]
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            _composerView.TextView.BecomeFirstResponder();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
        }
    }
}
