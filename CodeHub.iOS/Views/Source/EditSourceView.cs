using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using System.Threading.Tasks;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Source
{
	public class EditSourceView : ViewModelDialogViewController<EditSourceViewModel>
    {
		readonly ComposerView _composerView;
	
		public EditSourceView()
		{
			EdgesForExtendedLayout = UIRectEdge.None;
			Title = "Edit";
			_composerView = new ComposerView (ComputeComposerSize (RectangleF.Empty));

			View.AddSubview (_composerView);
		}
      
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => Commit());
            ViewModel.WhenAnyValue(x => x.Text).Subscribe(x => _composerView.Text = x);
		}

		private void Commit()
		{
			var composer = new LiteComposer { Title = "Commit Message" };
			var vm = (EditSourceViewModel)this.ViewModel;
			composer.Text = "Update " + vm.Path.Substring(vm.Path.LastIndexOf('/') + 1);
            var text = _composerView.Text;
            composer.ReturnAction += (s, e) => CommitThis(vm, composer, text, e);
			_composerView.TextView.BecomeFirstResponder ();
			NavigationController.PushViewController(composer, true);
		}

        /// <summary>
        /// Need another function because Xamarin generates an Invalid IL if used inline above
        /// </summary>
        private async Task CommitThis(EditSourceViewModel viewModel, LiteComposer composer, string content, string message)
        {
//            try
//            {
//                await this.DoWorkAsync("Commiting...", () => viewModel.Commit(content, message));
//                NavigationController.DismissViewController(true, null);
//            }
//            catch (Exception ex)
//            {
//                MonoTouch.Utilities.ShowAlert("Error", ex.Message);
//                composer.EnableSendButton = true;
//            }
        }

		void KeyboardWillShow (NSNotification notification)
		{
			var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
			if (nsValue == null) return;
			var kbdBounds = nsValue.RectangleFValue;
			UIView.Animate(0.25f, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, () =>
			_composerView.Frame = ComputeComposerSize(kbdBounds), null);
		}

		void KeyboardWillHide (NSNotification notification)
		{
			UIView.Animate(0.2, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, () =>
			_composerView.Frame = ComputeComposerSize(RectangleF.Empty), null);
		}

		RectangleF ComputeComposerSize (RectangleF kbdBounds)
		{
			var view = View.Bounds;
			return new RectangleF (0, 0, view.Width, view.Height-kbdBounds.Height);
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
			NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillHideNotification"), KeyboardWillHide);

			_composerView.TextView.BecomeFirstResponder ();
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			NSNotificationCenter.DefaultCenter.RemoveObserver(this);
		}

		private class ComposerView : UIView 
		{
			internal readonly UITextView TextView;

			public ComposerView (RectangleF bounds) : base (bounds)
			{
				TextView = new UITextView (RectangleF.Empty) {
					Font = UIFont.SystemFontOfSize (14),
				};

				// Work around an Apple bug in the UITextView that crashes
				if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
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

			void Resize (RectangleF bounds)
			{
				TextView.Frame = new RectangleF (0, 0, bounds.Width, bounds.Height);
			}

			public string Text { 
				get {
					return TextView.Text;
				}
				set {
					TextView.Text = value;
					TextView.SelectedRange = new NSRange(0, 0);
				}
			}
		}

    }
}

