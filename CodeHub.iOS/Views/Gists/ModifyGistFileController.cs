using System;
using System.Drawing;
using CodeHub.iOS;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public partial class ModifyGistFileController : UIViewController
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
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.BackButton, UIBarButtonItemStyle.Plain, (s, e) => NavigationController.PopViewControllerAnimated(true));
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Images.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => {

                var newName = Name.Text;
                var newContent = Text.Text;

                if (String.IsNullOrEmpty(newContent))
                {
                    //MonoTouch.Utilities.ShowAlert("No Content", "You cannot save a file without content!");
                    return;
                }

                try
                {
                    if (Save != null)
                        Save(newName, newContent);
                    NavigationController.PopViewControllerAnimated(true);
                }
				catch (Exception ex)
                {
					//MonoTouch.Utilities.ShowAlert("Error", ex.Message);
                    return;
                }
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (!string.IsNullOrEmpty(_name))
                Name.Text = _name;
            if (!string.IsNullOrEmpty(_content))
                Text.Text = _content;

            Text.Changed += HandleChanged;
            UpdateScrollContentSize();

            var v = NameView as TappableView;
            if (v != null)
            {
                v.Tapped = () => {
                    Name.BecomeFirstResponder();
                };
            }
        }

        void HandleChanged (object sender, EventArgs e)
        {
            UpdateScrollContentSize();
        }

        void UpdateScrollContentSize()
        {
            Scroll.ContentSize = new SizeF(View.Bounds.Width, Text.ContentSize.Height + offsetSize);
            var f = Text.Frame;
            var newHeight = Text.ContentSize.Height;
            if (newHeight < (Scroll.Frame.Height - offsetSize))
                newHeight = Scroll.Frame.Height - offsetSize;
            Text.Frame = new RectangleF(f.X, f.Y, f.Width, newHeight);
        }

        void KeyboardWillShow (NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            var kbdBounds = nsValue.RectangleFValue;
            Scroll.Frame = ComputeComposerSize (kbdBounds);
        }

        RectangleF ComputeComposerSize (RectangleF kbdBounds)
        {
            var view = View.Bounds;
            return new RectangleF (0, 0, view.Width, view.Height-kbdBounds.Height);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            Name.BecomeFirstResponder();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
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

