using CoreGraphics;
using CodeHub.Core.ViewModels.Accounts;
using Foundation;
using UIKit;
using CodeHub.iOS.Utilities;
using System;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public partial class AddAccountViewController : BaseViewController
    {
        private readonly UIColor ComponentTextColor = UIColor.White;
        private readonly UIColor EnterpriseBackgroundColor = UIColor.FromRGB(50, 50, 50);
        private readonly UIColor ComponentBackground = UIColor.FromRGB(0x3C, 0x3C, 0x3C);

        public AddAccountViewModel ViewModel { get; } = new AddAccountViewModel();

        public AddAccountViewController()
            : base("AddAccountView", null)
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem();

            Title = "Login";

            OnActivation(d =>
            {
                d(this.WhenAnyObservable(x => x.ViewModel.LoginCommand.IsExecuting)
                  .SubscribeStatus("Logging in..."));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var scopes = string.Join(", ", OctokitClientFactory.Scopes);
            DescriptionLabel.Text = string.Format("The provided Personal Access Token must allow access to the following scopes: {0}", scopes);
            DescriptionLabel.TextColor = ComponentTextColor;

            AuthenticationSelector.TintColor = ComponentTextColor.ColorWithAlpha(0.9f);

            View.BackgroundColor = EnterpriseBackgroundColor;
            Logo.Image = Images.Logos.EnterpriseMascot;

            LoginButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            LoginButton.Enabled = true;

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.2f;

            var attributes = new UIStringAttributes {
                ForegroundColor = UIColor.LightGray,
            };

            Domain.AttributedPlaceholder = new NSAttributedString("Domain", attributes);
            User.AttributedPlaceholder = new NSAttributedString("Username", attributes);
            Password.AttributedPlaceholder = new NSAttributedString("Password", attributes);
            TokenTextField.AttributedPlaceholder = new NSAttributedString("Token", attributes);

            foreach (var i in new [] { Domain, User, Password, TokenTextField })
            {
                i.Layer.BorderColor = UIColor.Black.CGColor;
                i.Layer.BorderWidth = 1;
                i.Layer.CornerRadius = 4;
            }

            SelectAuthenticationScheme(0);

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

            OnActivation(d =>
            {
                d(User.GetChangedObservable()
                  .Subscribe(x => ViewModel.Username = x));
                
                d(Password.GetChangedObservable()
                  .Subscribe(x => ViewModel.Password = x));
                
                d(Domain.GetChangedObservable()
                  .Subscribe(x => ViewModel.Domain = x));

                d(TokenTextField.GetChangedObservable()
                  .Subscribe(x => ViewModel.Token = x));
                
                d(LoginButton.GetClickedObservable()
                  .InvokeReactiveCommand(ViewModel.LoginCommand));

                d(AuthenticationSelector.GetChangedObservable()
                  .Do(x => ViewModel.TokenAuthentication = x == 1)
                  .Subscribe(SelectAuthenticationScheme));

                d(this.WhenAnyObservable(x => x.ViewModel.LoginCommand.CanExecute)
                  .Subscribe(x => LoginButton.Enabled = x));

                d(this.WhenAnyValue(x => x.ViewModel.TokenAuthentication)
                  .Subscribe(x => AuthenticationSelector.SelectedSegment = x ? 1 : 0));
            });
        }

        private void SelectAuthenticationScheme(int scheme)
        {
            UIView.Animate(0.3, () =>
            {
                if (scheme == 0)
                {
                    TokenTextField.Hidden = true;
                    User.Hidden = false;
                    Password.Hidden = false;
                }
                else
                {
                    TokenTextField.Hidden = false;
                    User.Hidden = true;
                    Password.Hidden = true;
                }
            });

            UIView.Animate(0.3, 0, UIViewAnimationOptions.TransitionCrossDissolve,
                           () => DescriptionLabel.Alpha = scheme == 0 ? 0 : 1, null);
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

