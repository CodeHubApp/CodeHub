using System;
using MonoTouch.UIKit;
using RedPlum;
using System.Threading;
using MonoTouch;
using CodeHub.Data;
using CodeFramework.Views;
using System.Linq;
using System.Drawing;
using MonoTouch.Foundation;

namespace CodeHub.ViewControllers
{
    public partial class LoginViewController : UIViewController
    {
        private bool _enterprise;
        private Account _attemptedAccount;

        public LoginViewController(bool enterprise)
            : base("LoginViewController", null)
        {
            Title = "Login".t();
            _enterprise = enterprise;
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.BackButton, () => NavigationController.PopViewControllerAnimated(true)));
        }

        public LoginViewController(Account attemptedAccount)
            : this(attemptedAccount.Domain != null)
        {
            _attemptedAccount = attemptedAccount;
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            try
            {
                var color = Utilities.CreateRepeatingBackground();
                if (color != null)
                    View.BackgroundColor = color;
            }
            catch (Exception e)
            {
                Utilities.Analytics.TrackException(false, e.Message);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Logo.Image = Images.Logos.GitHub;

            if (_enterprise)
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

            if (_attemptedAccount != null)
            {
                Domain.Text = _attemptedAccount.Domain;
                User.Text = _attemptedAccount.Username;
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
                DoLogin();
                return true;
            };

            LoginButton.TouchUpInside += (object sender, EventArgs e) => DoLogin();
            ScrollView.ContentSize = new SizeF(View.Frame.Width, LoginButton.Frame.Bottom + 10f);
        }

        private void DoLogin()
        {
            Utils.Login.LoginAccount(_enterprise ? Domain.Text : null, User.Text, Password.Text, this);
        }

        NSObject hideNotification, showNotification;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            hideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
            showNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(hideNotification);
            NSNotificationCenter.DefaultCenter.RemoveObserver(showNotification);
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

