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

            var saveButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem { Image = Images.Buttons.SaveButton };

            _textView.Frame = ComputeComposerSize(CGRect.Empty);
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
            var composer = new LiteComposer { Title = "Commit Message" };
            composer.Text = "Update " + ViewModel.Path.Substring(ViewModel.Path.LastIndexOf('/') + 1);
            var text = _textView.Text;
            composer.ReturnAction += (s, e) => CommitThis(ViewModel, composer, text, e);
            _textView.BecomeFirstResponder ();
            NavigationController.PushViewController(composer, true);
        }

        /// <summary>
        /// Need another function because Xamarin generates an Invalid IL if used inline above
        /// </summary>
        private async Task CommitThis(EditSourceViewModel viewModel, LiteComposer composer, string content, string message)
        {
            try
            {
                await this.DoWorkAsync("Commiting...", () => viewModel.Commit(content, message));
                NavigationController.DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                AlertDialogService.ShowAlert("Error", ex.Message);
                composer.EnableSendButton = true;
            }
        }

        void KeyboardWillShow (NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            var kbdBounds = nsValue.RectangleFValue;
            UIView.Animate(0.25f, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, () =>
                _textView.Frame = ComputeComposerSize(kbdBounds), null);
        }

        void KeyboardWillHide (NSNotification notification)
        {
            UIView.Animate(0.2, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, () =>
                _textView.Frame = ComputeComposerSize(CGRect.Empty), null);
        }

        CGRect ComputeComposerSize (CGRect kbdBounds)
        {
            var view = View.Bounds;
            return new CGRect (0, 0, view.Width, view.Height-kbdBounds.Height);
        }

        NSObject _hideNotification, _showNotification;
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            _hideNotification = NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillHideNotification"), KeyboardWillHide);
            _textView.BecomeFirstResponder ();
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

