using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace CodeFramework.iOS.ViewControllers
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

			public ComposerView (CGRect bounds) : base (bounds)
			{
				TextView = new UITextView (CGRect.Empty) {
					Font = UIFont.SystemFontOfSize (18),
				};

				// Work around an Apple bug in the UITextView that crashes
				if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR)
					TextView.AutocorrectionType = UITextAutocorrectionType.No;

				AddSubview (TextView);
			}


			internal void Reset (string text)
			{
				TextView.Text = text;
			}

			public override void LayoutSubviews ()
			{
				Resize (Bounds);
			}

			void Resize (CGRect bounds)
			{
				TextView.Frame = new CGRect (0, 0, bounds.Width, bounds.Height);
			}

			public string Text { 
				get {
					return TextView.Text;
				}
				set {
					TextView.Text = value;
				}
			}
		}

		public LiteComposer () : base (null, null)
		{
			Title = "New Comment".t();
			EdgesForExtendedLayout = UIRectEdge.None;
			// Navigation Bar

			var close = new UIBarButtonItem (Theme.CurrentTheme.BackButton, UIBarButtonItemStyle.Plain, (s, e) => CloseComposer());
			NavigationItem.LeftBarButtonItem = close;
			SendItem = new UIBarButtonItem (Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => PostCallback());
			NavigationItem.RightBarButtonItem = SendItem;

			// Composer
			_composerView = new ComposerView (ComputeComposerSize (CGRect.Empty));

			View.AddSubview (_composerView);
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

		public void CloseComposer ()
		{
			SendItem.Enabled = true;
			NavigationController.PopViewController(true);
		}

		void PostCallback ()
		{
			SendItem.Enabled = false;
            var handler = ReturnAction;
            if (handler != null)
                handler(this, Text);
		}

		void KeyboardWillShow (NSNotification notification)
		{
			var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
			if (nsValue == null) return;
			var kbdBounds = nsValue.RectangleFValue;
			_composerView.Frame = ComputeComposerSize (kbdBounds);
		}

		CGRect ComputeComposerSize (CGRect kbdBounds)
		{
			var view = View.Bounds;
			return new CGRect (0, 0, view.Width, view.Height-kbdBounds.Height);
		}

		[Obsolete]
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
			_composerView.TextView.BecomeFirstResponder ();
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			NSNotificationCenter.DefaultCenter.RemoveObserver(this);
		}
	}
}
