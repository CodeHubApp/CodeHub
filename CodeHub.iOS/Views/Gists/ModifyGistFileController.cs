using System;
using CoreGraphics;
using CodeHub.iOS;
using Foundation;
using UIKit;
using CodeHub.iOS.Services;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.iOS.Views
{
    public partial class ModifyGistFileController : BaseViewController
    {
        public Action<string, string> Save;
        const float offsetSize = 44f + 8f + 1f;
        string _name;
        string _content;

        public ModifyGistFileController(string name = null, string content = null) 
            : base ("ModifyGistFileController", null)
        {
            _name = name;
            _content = content;

            Title = "New File";
            var cancelButton = NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Theme.CurrentTheme.BackButton };
            var saveButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem { Image = Theme.CurrentTheme.SaveButton };

            OnActivation(d =>
            {
                d(cancelButton.GetClickedObservable().Subscribe(_ => NavigationController.PopViewController(true)));
                d(saveButton.GetClickedObservable().Subscribe(_ => SaveClicked()));
            });
        }

        private void SaveClicked()
        {
            var newName = Name.Text;
            var newContent = Text.Text;

            if (String.IsNullOrEmpty(newContent))
            {
                AlertDialogService.ShowAlert("No Content", "You cannot save a file without content!");
                return;
            }

            try
            {
                Save?.Invoke(newName, newContent);
                NavigationController.PopViewController(true);
            }
            catch (Exception ex)
            {
                AlertDialogService.ShowAlert("Error", ex.Message);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (!string.IsNullOrEmpty(_name))
                Name.Text = _name;
            if (!string.IsNullOrEmpty(_content))
                Text.Text = _content;

            var weakThis = new WeakReference<ModifyGistFileController>(this);

            Text.Changed += (sender, e) => weakThis.Get()?.UpdateScrollContentSize();
            UpdateScrollContentSize();

            var v = NameView as TappableView;
            if (v != null)
            {
                v.Tapped = () => weakThis.Get().Name.BecomeFirstResponder();
            }
        }

        void UpdateScrollContentSize()
        {
            Scroll.ContentSize = new CGSize(View.Bounds.Width, Text.ContentSize.Height + offsetSize);
            var f = Text.Frame;
            var newHeight = Text.ContentSize.Height;
            if (newHeight < (Scroll.Frame.Height - offsetSize))
                newHeight = Scroll.Frame.Height - offsetSize;
            Text.Frame = new CGRect(f.X, f.Y, f.Width, newHeight);
        }

        void KeyboardWillShow (NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            var kbdBounds = nsValue.RectangleFValue;
            Scroll.Frame = ComputeComposerSize (kbdBounds);
        }

        CGRect ComputeComposerSize (CGRect kbdBounds)
        {
            var view = View.Bounds;
            return new CGRect (0, 0, view.Width, view.Height-kbdBounds.Height);
        }

        NSObject _showNotification;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            Name.BecomeFirstResponder();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (_showNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            UpdateScrollContentSize();
        }

        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }

        [Register("TappableView")]
        public class TappableView : UIView
        {
            public Action Tapped;

            public TappableView()
            {
            }

            public TappableView(IntPtr handle)
                : base(handle)
            {
            }

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);
                if (Tapped != null)
                    Tapped();
            }
        }
    }
}

