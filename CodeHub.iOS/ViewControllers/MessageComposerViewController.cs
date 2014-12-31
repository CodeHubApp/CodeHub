using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using Xamarin.Utilities.ViewModels;

namespace CodeHub.iOS.ViewControllers
{

    public abstract class MessageComposerViewController<TViewModel> : MessageComposerViewController, IViewFor<TViewModel> where TViewModel : class
    {
        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set { this.RaiseAndSetIfChanged(ref _viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return _viewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        protected MessageComposerViewController()
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };
            this.WhenAnyValue(x => x.ViewModel).OfType<ILoadableViewModel>().Subscribe(x => x.LoadCommand.ExecuteIfCan());
            this.WhenAnyValue(x => x.ViewModel).OfType<IProvidesTitle>().Select(x => x.WhenAnyValue(y => y.Title)).Switch().Subscribe(x => Title = x);
            this.WhenActivated(d => { });
        }
    }

    public class MessageComposerViewController : ReactiveViewController
    {
        private RectangleF _keyboardBounds = RectangleF.Empty;

        public UITextView TextView { get; private set; }

        public MessageComposerViewController()
        {
            EdgesForExtendedLayout = UIRectEdge.None;
            TextView = new UITextView(RectangleF.Empty);
            TextView.Font = UIFont.FromName("Courier", 16f);

            // Work around an Apple bug in the UITextView that crashes
            if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
                TextView.AutocorrectionType = UITextAutocorrectionType.No;
        }

        public string Text
        {
            get { return TextView.Text; }
            set
            {
                TextView.Text = value;
                TextView.SelectedRange = new NSRange(0, 0);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;
            Add(TextView);
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
            _keyboardBounds = RectangleF.Empty;
            UIView.Animate(0.2, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, ResizeTextView, null);
        }

        private void ResizeTextView()
        {
            TextView.Frame = new RectangleF(0, 0, View.Bounds.Width, View.Bounds.Height - _keyboardBounds.Height);
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
            NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillHideNotification"), KeyboardWillHide);
            ResizeTextView();
            TextView.BecomeFirstResponder();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
        }
    }
}
