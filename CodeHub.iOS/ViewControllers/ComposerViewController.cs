using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class ComposerViewController<TViewModel> : ReactiveViewController, IViewFor<TViewModel> where TViewModel : class, IComposerViewModel
    {
        protected UIBarButtonItem SendItem;
        protected readonly UITextView TextView;
        protected UIView ScrollingToolbarView;
        private readonly UIImage _normalButtonImage;
        private readonly UIImage _pressedButtonImage;
        private NSObject _observer;

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            _observer = NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidChangeNotification,
                notification => ViewModel.Text = TextView.Text);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (_observer != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_observer);
                _observer = null;
            }
        }

        protected ComposerViewController()
            : base(null, null)
        {

            Title = "New Comment";
            EdgesForExtendedLayout = UIRectEdge.None;

            TextView = new UITextView
            {
                Font = UIFont.SystemFontOfSize(18),
                AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth
            };

            // Work around an Apple bug in the UITextView that crashes
            if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
                TextView.AutocorrectionType = UITextAutocorrectionType.No;

            _normalButtonImage = ImageFromColor(UIColor.White);
            _pressedButtonImage = ImageFromColor(UIColor.FromWhiteAlpha(0.0f, 0.4f));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TextView.Frame = ComputeComposerSize(RectangleF.Empty);
            View.AddSubview(TextView);
        }

        private UIImage ImageFromColor(UIColor color)
        {
            UIGraphics.BeginImageContext(new SizeF(1, 1));
            var context = UIGraphics.GetCurrentContext();
            context.SetFillColor(color.CGColor);
            context.FillRect(new RectangleF(0, 0, 1, 1));
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
            btn.Frame = new RectangleF(0, 0, 32, 32);
            btn.SetTitle(title, UIControlState.Normal);
            btn.BackgroundColor = UIColor.White;
            btn.Font = UIFont.SystemFontOfSize(fontSize);
            btn.Layer.CornerRadius = 7f;
            btn.Layer.MasksToBounds = true;
            btn.AdjustsImageWhenHighlighted = false;
            btn.TouchUpInside += (sender, e) => action();
            return btn;
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

        public void SetAccesoryButtons(ICollection<UIButton> buttons)
        {
            foreach (var button in buttons)
            {
                button.SetBackgroundImage(_normalButtonImage, UIControlState.Normal);
                button.SetBackgroundImage(_pressedButtonImage, UIControlState.Highlighted);
            }

            var height = CalculateHeight(UIApplication.SharedApplication.StatusBarOrientation);
            ScrollingToolbarView = new ScrollingToolbarView(new RectangleF(0, 0, View.Bounds.Width, height), buttons);
            ScrollingToolbarView.BackgroundColor = UIColor.FromWhiteAlpha(0.7f, 1.0f);
            TextView.InputAccessoryView = ScrollingToolbarView;
        }

        void KeyboardWillShow(NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey(UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            var kbdBounds = nsValue.RectangleFValue;
            UIView.Animate(1.0f, 0, UIViewAnimationOptions.CurveEaseIn, () => TextView.Frame = ComputeComposerSize(kbdBounds), null);
        }

        void KeyboardWillHide(NSNotification notification)
        {
            TextView.Frame = ComputeComposerSize(new RectangleF(0, 0, 0, 0));
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
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillHideNotification"), KeyboardWillHide);
            TextView.BecomeFirstResponder();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        public TViewModel ViewModel { get; set; }
    }
}
