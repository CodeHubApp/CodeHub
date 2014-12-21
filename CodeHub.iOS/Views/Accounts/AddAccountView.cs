using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Accounts;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.Services;
using Xamarin.Utilities.Factories;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class AddAccountView : ReactiveViewController<AddAccountViewModel>
    {
        private readonly IStatusIndicatorService _statusIndicatorService;
        private readonly IAlertDialogFactory _alertDialogService;

        public AddAccountView(IStatusIndicatorService statusIndicatorService, IAlertDialogFactory alertDialogService)
            : base("AddAccountView", null)
        {
            _statusIndicatorService = statusIndicatorService;
            _alertDialogService = alertDialogService;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.LoginCommand.IsExecuting.Skip(1).Subscribe(x =>
			{
				if (x)
                    _statusIndicatorService.Show("Logging in...");
				else
                    _statusIndicatorService.Hide();
			});
            
		    ViewModel.LoginCommand.ThrownExceptions.Subscribe(x =>
		    {
		        if (x is LoginService.TwoFactorRequiredException)
		        {
		            _alertDialogService.PromptTextBox("Two Factor Authentication",
		                "This account requires a two factor authentication code", string.Empty, "Ok")
		                .ContinueWith(t =>
		                {
		                    ViewModel.TwoFactor = t.Result;
                            ViewModel.LoginCommand.ExecuteIfCan();
		                }, TaskContinuationOptions.OnlyOnRanToCompletion);
		        }
		    });

            User.EditingChanged += (sender, args) => ViewModel.Username = User.Text;
            ViewModel.WhenAnyValue(x => x.Username).Subscribe(x => User.Text = x);

            Password.EditingChanged += (sender, args) => ViewModel.Password = Password.Text;
            ViewModel.WhenAnyValue(x => x.Password).Subscribe(x => Password.Text = x);

            Domain.ValueChanged += (sender, args) => ViewModel.Domain = Domain.Text;
            ViewModel.WhenAnyValue(x => x.Domain).Subscribe(x => Domain.Text = x);

            LoginButton.TouchUpInside += (sender, args) => ViewModel.LoginCommand.ExecuteIfCan();
            ViewModel.LoginCommand.CanExecuteObservable.Subscribe(x => LoginButton.Enabled = x);

			View.BackgroundColor = UIColor.FromRGB(239, 239, 244);
            Logo.Image = Images.Logos.GitHub;

            if (ViewModel.IsEnterprise)
            {
                LoginButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                LoginButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            }
            else
            {
                LoginButton.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

                //Hide the domain, slide everything up
                Domain.Hidden = true;
                var f = User.Frame;
                f.Y -= 39;
                User.Frame = f;

                var p = Password.Frame;
                p.Y -= 39;
                Password.Frame = p;

                var l = LoginButton.Frame;
                l.Y -= 39;
                LoginButton.Frame = l;
            }

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new SizeF(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.3f;

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


            ScrollView.ContentSize = new SizeF(View.Frame.Width, LoginButton.Frame.Bottom + 10f);
        }

        NSObject _hideNotification, _showNotification;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _hideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_hideNotification);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }

        private void OnKeyboardNotification (NSNotification notification)
        {
            if (IsViewLoaded) {

                //Check if the keyboard is becoming visible
                bool visible = notification.Name == UIKeyboard.WillShowNotification;

                //Start an animation, using values from the keyboard
                UIView.BeginAnimations ("AnimateForKeyboard");
                UIView.SetAnimationBeginsFromCurrentState (true);
                UIView.SetAnimationDuration (UIKeyboard.AnimationDurationFromNotification (notification));
                UIView.SetAnimationCurve ((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification (notification));

                //Pass the notification, calculating keyboard height, etc.
                bool landscape = InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
                if (visible) {
                    var keyboardFrame = UIKeyboard.FrameEndFromNotification (notification);

                    OnKeyboardChanged (visible, landscape ? keyboardFrame.Width : keyboardFrame.Height);
                } else {
                    var keyboardFrame = UIKeyboard.FrameBeginFromNotification (notification);

                    OnKeyboardChanged (visible, landscape ? keyboardFrame.Width : keyboardFrame.Height);
                }

                //Commit the animation
                UIView.CommitAnimations (); 
            }
        }

        /// <summary>
        /// Override this method to apply custom logic when the keyboard is shown/hidden
        /// </summary>
        /// <param name='visible'>
        /// If the keyboard is visible
        /// </param>
        /// <param name='keyboardHeight'>
        /// Calculated height of the keyboard (width not generally needed here)
        /// </param>
        protected virtual void OnKeyboardChanged (bool visible, float keyboardHeight)
        {
            if (visible)
            {
                var frame = ScrollView.Frame;
                frame.Height -= keyboardHeight;
                ScrollView.Frame = frame;
            }
            else
            {
                var frame = ScrollView.Frame;
                frame.Height = View.Bounds.Height;
                ScrollView.Frame = frame;
            }
        }
    }
}

