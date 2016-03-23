using CoreGraphics;
using CodeHub.Core.ViewModels.Accounts;
using Foundation;
using UIKit;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.ViewControllers;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;
using System;
using GitHubSharp;
using System.Linq;
using System.Reactive.Threading.Tasks;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public partial class AddAccountViewController : BaseViewController
    {
        private readonly UIColor EnterpriseBackgroundColor = UIColor.FromRGB(50, 50, 50);

        public AddAccountViewModel ViewModel { get; }

        public AddAccountViewController()
            : base("AddAccountView", null)
        {
            ViewModel = new AddAccountViewModel(Mvx.Resolve<IApplicationService>(), Mvx.Resolve<ILoginFactory>());
            ViewModel.Init(new AddAccountViewModel.NavObject());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Login";


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

            OnActivation(d =>
                {
                    d(User.GetChangedObservable().Subscribe(x => ViewModel.Username = x));
                    d(Password.GetChangedObservable().Subscribe(x => ViewModel.Password = x));
                    d(Domain.GetChangedObservable().Subscribe(x => ViewModel.Domain = x));
                    d(LoginButton.GetClickedObservable().BindCommand(ViewModel.LoginCommand));
                    d(ViewModel.Bind(x => x.IsLoggingIn).SubscribeStatus("Logging in..."));
                    d(ViewModel.LoginCommand.ThrownExceptions.Subscribe(HandleLoginException));
                });
        }

        private void HandleLoginException(Exception e)
        {
            var alert = Mvx.Resolve<IAlertDialogService>();

            var authException = e as UnauthorizedException;
            if (authException != null && authException.Headers.Contains("X-GitHub-OTP"))
            {
                alert.PromptTextBox("Authentication Error", "Please provide the two-factor authentication code for this account.", string.Empty, "Login")
                    .ToObservable()
                    .Subscribe(x => {
                        ViewModel.TwoFactor = x;
                        ViewModel.LoginCommand.ExecuteIfCan();
                    });
            }
            else
            {
                alert.Alert("Unable to Login!", "Unable to login user " + ViewModel.Username + ": " + e.Message);
            }
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

