using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class LiteComposer : BaseViewController
    {
        private readonly UITextView _textView;
        private readonly UIBarButtonItem _sendButton;

        public event EventHandler<string> ReturnAction;

        public bool EnableSendButton
        {
            get { return _sendButton.Enabled; }
            set { _sendButton.Enabled = value; }
        }

        public LiteComposer () : base (null, null)
        {
            Title = "New Comment";
            EdgesForExtendedLayout = UIRectEdge.None;

            _textView = new UITextView()
            {
                Font = UIFont.PreferredBody
            };

            var close = new UIBarButtonItem { Image = Images.Buttons.BackButton };
            NavigationItem.LeftBarButtonItem = close;
            _sendButton = new UIBarButtonItem { Image = Images.Buttons.SaveButton };
            NavigationItem.RightBarButtonItem = _sendButton;

            OnActivation(d =>
            {
                d(close.GetClickedObservable().Subscribe(_ => CloseComposer()));
                d(_sendButton.GetClickedObservable().Subscribe(_ => PostCallback()));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _textView.Frame = ComputeComposerSize(CGRect.Empty);
            View.AddSubview (_textView);
        }

        public string Text
        {
            get { return _textView.Text; }
            set { _textView.Text = value; }
        }

        public string ActionButtonText 
        {
            get { return NavigationItem.RightBarButtonItem.Title; }
            set { NavigationItem.RightBarButtonItem.Title = value; }
        }

        public void CloseComposer ()
        {
            _sendButton.Enabled = true;
            NavigationController.PopViewController(true);
        }

        void PostCallback ()
        {
            _sendButton.Enabled = false;
            var handler = ReturnAction;
            if (handler != null)
                handler(this, Text);
        }

        void KeyboardWillShow (NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            var kbdBounds = nsValue.RectangleFValue;
            _textView.Frame = ComputeComposerSize (kbdBounds);
        }

        CGRect ComputeComposerSize (CGRect kbdBounds)
        {
            var view = View.Bounds;
            return new CGRect (0, 0, view.Width, view.Height-kbdBounds.Height);
        }

        NSObject _showNotification;
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            _textView.BecomeFirstResponder ();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (_showNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }
    }
}
