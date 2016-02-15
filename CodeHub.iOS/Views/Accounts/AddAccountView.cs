using CoreGraphics;
using CodeHub.Core.ViewModels.Accounts;
using Foundation;
using UIKit;
using CodeHub.iOS.Utilities;
using System;
using MvvmCross.iOS.Views;
using MvvmCross.Binding.BindingContext;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class AddAccountView : MvxViewController
    {
        private readonly UIColor EnterpriseBackgroundColor = UIColor.FromRGB(50, 50, 50);

        public new AddAccountViewModel ViewModel
        {
            get { return (AddAccountViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public AddAccountView()
            : base("AddAccountView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Login";

            var set = this.CreateBindingSet<AddAccountView, AddAccountViewModel>();
            set.Bind(User).To(x => x.Username);
            set.Bind(Password).To(x => x.Password);
            set.Bind(Domain).To(x => x.Domain);
            set.Bind(LoginButton).To(x => x.LoginCommand);
            set.Apply();

            ViewModel.Bind(x => x.IsLoggingIn).SubscribeStatus("Logging in...");

            View.BackgroundColor = EnterpriseBackgroundColor;
            Logo.Image = Images.Logos.EnterpriseMascot;

            LoginButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            LoginButton.Enabled = true;

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.3f;

            var attributes = new UIStringAttributes {
                ForegroundColor = UIColor.LightGray,
            };

            Domain.AttributedPlaceholder = new NSAttributedString("Domain", attributes);
            User.AttributedPlaceholder = new NSAttributedString("Username", attributes);
            Password.AttributedPlaceholder = new NSAttributedString("Password", attributes);

            foreach (var i in new [] { Domain, User, Password })
            {
                i.Layer.BorderColor = UIColor.Black.CGColor;
                i.Layer.BorderWidth = 1;
                i.Layer.CornerRadius = 4;
            }

            Domain.ShouldReturn = delegate {
                User.BecomeFirstResponder();
                return true;
            };

            User.ShouldReturn = delegate {
                Password.BecomeFirstResponder();
                return true;
            };
            Password.ShouldReturn = delegate {
                Password.ResignFirstResponder();
                LoginButton.SendActionForControlEvents(UIControlEvent.TouchUpInside);
                return true;
            };
        }

        NSObject _hideNotification, _showNotification;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _hideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardHideNotification);
            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_hideNotification);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }

        private void OnKeyboardHideNotification(NSNotification notification)
        {
            ScrollView.ContentInset = UIEdgeInsets.Zero;
            ScrollView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
        }

        private void OnKeyboardNotification (NSNotification notification)
        {
            var keyboardFrame = UIKeyboard.FrameEndFromNotification (notification);
            var inset = new UIEdgeInsets(0, 0, keyboardFrame.Height, 0);
            ScrollView.ContentInset = inset;
            ScrollView.ScrollIndicatorInsets = inset;
        }
    }
}

