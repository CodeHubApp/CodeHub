using System;
using UIKit;
using CodeHub.Core.ViewModels.Accounts;
using ReactiveUI;
using CoreGraphics;
using CodeHub.Core.Utilities;
using System.Reactive.Disposables;
using Foundation;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public partial class EnterpriseOAuthTokenLoginViewController : BaseViewController
    {
        private readonly UIColor BackgroundColor = UIColor.FromRGB(50, 50, 50);
        private readonly UIColor ComponentTextColor = UIColor.White;
        private readonly UIColor ComponentPlaceholderColor = UIColor.FromWhiteAlpha(0.8f, 1f);
        private readonly UIColor ComponentBackgroundColor = UIColor.FromRGB(0x3C, 0x3C, 0x3C);

        public EnterpriseOAuthTokenLoginViewModel ViewModel { get; } = new EnterpriseOAuthTokenLoginViewModel();

        public EnterpriseOAuthTokenLoginViewController()
            : base("EnterpriseOAuthTokenLoginView", null)
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem();

            Title = "Token Login";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var scopes = string.Join(", ", OctokitClientFactory.Scopes);
            DescriptionLabel.Text = string.Format("The provided Personal Access Token must allow access to the following scopes: {0}", scopes);
            DescriptionLabel.TextColor = ComponentTextColor;

            TokenText.AttributedPlaceholder = new NSAttributedString("Token", foregroundColor: ComponentPlaceholderColor);
            TokenText.BackgroundColor = ComponentBackgroundColor;
            TokenText.TextColor = ComponentTextColor;

            DomainText.AttributedPlaceholder = new NSAttributedString("Domain", foregroundColor: ComponentPlaceholderColor);
            DomainText.BackgroundColor = ComponentBackgroundColor;
            DomainText.TextColor = ComponentTextColor;

            View.BackgroundColor = BackgroundColor;
            LogoImageView.Image = Images.Logos.EnterpriseMascot;

            LoginButton.SetTitleColor(ComponentTextColor, UIControlState.Normal);
            LoginButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.2f;

            foreach (var i in new[] { TokenText, DomainText })
            {
                i.Layer.BorderColor = UIColor.Black.CGColor;
                i.Layer.BorderWidth = 1;
                i.Layer.CornerRadius = 4;
            }

            OnActivation(d => {
                d(TokenText.GetChangedObservable().Subscribe(x => ViewModel.Token = x));
                d(ViewModel.WhenAnyValue(x => x.Token).Subscribe(x => TokenText.Text = x));

                d(DomainText.GetChangedObservable().Subscribe(x => ViewModel.Domain = x));
                d(ViewModel.WhenAnyValue(x => x.Domain).Subscribe(x => DomainText.Text = x));

                d(LoginButton.GetClickedObservable().InvokeCommand(ViewModel.LoginCommand));
                d(ViewModel.LoginCommand.CanExecute.Subscribe(x => LoginButton.Enabled = x));

                DomainText.ShouldReturn = delegate {
                    TokenText.BecomeFirstResponder();
                    return true;
                };

                TokenText.ShouldReturn = delegate {
                    TokenText.ResignFirstResponder();
                    LoginButton.SendActionForControlEvents(UIControlEvent.TouchUpInside);
                    return true;
                };

                d(Disposable.Create(() => {
                    DomainText.ShouldReturn = null;
                    TokenText.ShouldReturn = null;
                }));
            });
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

        private void OnKeyboardNotification(NSNotification notification)
        {
            var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);
            var inset = new UIEdgeInsets(0, 0, keyboardFrame.Height, 0);
            ScrollView.ContentInset = inset;
            ScrollView.ScrollIndicatorInsets = inset;
        }
    }
}

