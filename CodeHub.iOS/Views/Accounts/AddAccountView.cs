using CoreGraphics;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;
using CodeHub.Core.ViewModels.Accounts;
using Foundation;
using UIKit;
using CodeFramework.iOS.Utils;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using System;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class AddAccountView : MvxViewController
    {
		private readonly IHud _hud;

        public new AddAccountViewModel ViewModel
        {
            get { return (AddAccountViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public AddAccountView()
            : base("AddAccountView", null)
        {
            Title = "Login".t();
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.BackButton, UIBarButtonItemStyle.Plain, (s, e) => NavigationController.PopViewController(true));
			_hud = this.CreateHud();
        }

        public override void ViewDidLoad()
        {
            var set = this.CreateBindingSet<AddAccountView, AddAccountViewModel>();
            set.Bind(User).To(x => x.Username);
            set.Bind(Password).To(x => x.Password);
            set.Bind(Domain).To(x => x.Domain);
            set.Bind(LoginButton).To(x => x.LoginCommand);
            set.Apply();

            base.ViewDidLoad();

			ViewModel.Bind(x => x.IsLoggingIn, x =>
			{
				if (x)
					_hud.Show("Logging in...");
				else
					_hud.Hide();
			});

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
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
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


            ScrollView.ContentSize = new CGSize(View.Frame.Width, LoginButton.Frame.Bottom + 10f);
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
        protected virtual void OnKeyboardChanged (bool visible, nfloat keyboardHeight)
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

