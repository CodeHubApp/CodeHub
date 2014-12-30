using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Utilities.ViewControllers;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Gists
{
    public partial class ModifyGistFileController : ReactiveViewController<ModifyGistViewModel>
    {
        const float offsetSize = 44f + 8f + 1f;

        public ModifyGistFileController() 
            : base ("ModifyGistFileController", null)
        {
            this.WhenAnyValue(x => x.ViewModel.SaveCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = (x == null ? null : x.ToBarButtonItem(UIBarButtonSystemItem.Save)));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Name.EditingChanged += (sender, e) => 
                ViewModel.Filename = Name.Text;
            this.WhenAnyValue(x => x.ViewModel.Filename).Subscribe(x => Name.Text = x);

            Text.Changed += (sender, e) => 
                ViewModel.Description = Text.Text;
            this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => Text.Text = x);

            Text.Changed += HandleChanged;
            UpdateScrollContentSize();

            var v = NameView as TappableView;
            if (v != null)
            {
                v.Tapped = () => Name.BecomeFirstResponder();
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

