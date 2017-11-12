using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using UIKit;
using CoreGraphics;
using Foundation;
using CodeHub.iOS.Utilities;
using System.Threading.Tasks;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.Views.Source
{
    public class EditSourceView : BaseViewController
    {
        private readonly UITextView _textView;

        public EditSourceViewModel ViewModel { get; }
    
        public EditSourceView()
        {
            ViewModel = new EditSourceViewModel();
            EdgesForExtendedLayout = UIRectEdge.None;
            Title = "Edit";

            _textView = new UITextView {
                Font = UIFont.FromName("Courier", UIFont.PreferredBody.PointSize),
                SpellCheckingType = UITextSpellCheckingType.No,
                AutocorrectionType = UITextAutocorrectionType.No,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight 
            };
        }
      
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var saveButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save);

            _textView.Frame = new CGRect(CGPoint.Empty, View.Bounds.Size);
            View.AddSubview(_textView);

            OnActivation(d =>
            {
                d(saveButton.GetClickedObservable().Subscribe(_ => Commit()));
                d(ViewModel.Bind(x => x.Text).Subscribe(x => {
                    _textView.Text = x;
                    _textView.SelectedRange = new NSRange(0, 0);
                }));
            });

            ViewModel.LoadCommand.Execute(null);
        }

        private void Commit()
        {
            var content = _textView.Text;

            var composer = new Composer
            {
                Title = "Commit Message",
                Text = "Update " + ViewModel.Path.Substring(ViewModel.Path.LastIndexOf('/') + 1)
            };

            composer.PresentAsModal(this, text => CommitThis(content, text).ToBackground());
        }

        /// <summary>
        /// Need another function because Xamarin generates an Invalid IL if used inline above
        /// </summary>
        private async Task CommitThis(string content, string message)
        {
            try
            {
                UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
                await this.DoWorkAsync("Commiting...", () => ViewModel.Commit(content, message));
                this.PresentingViewController?.DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                AlertDialogService.ShowAlert("Error", ex.Message);
            }
            finally
            {
                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
            }
        }

        void KeyboardChange(NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey(UIKeyboard.FrameEndUserInfoKey) as NSValue;
            if (nsValue == null) return;

            var kbdBounds = nsValue.RectangleFValue;
            var keyboard = View.Window.ConvertRectToView(kbdBounds, View);

            UIView.Animate(
                1.0f, 0, UIViewAnimationOptions.CurveEaseIn,
                () => _textView.Frame = new CGRect(0, 0, View.Bounds.Width, keyboard.Top), null);
        }

        NSObject _hideNotification, _showNotification;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("UIKeyboardWillShowNotification"), KeyboardChange);

            _hideNotification = NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("UIKeyboardWillHideNotification"), KeyboardChange);

            _textView.BecomeFirstResponder();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (_hideNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_hideNotification);
            if (_showNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }
    }
}

