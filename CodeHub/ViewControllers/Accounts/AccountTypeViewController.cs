using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch;
using CodeFramework.Views;

namespace CodeHub.ViewControllers
{
    public partial class AccountTypeViewController : UIViewController
    {
        public AccountTypeViewController()
			: base ("AccountTypeViewController", null)
        {
            Title = "Account";
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.BackButton, () => NavigationController.PopViewControllerAnimated(true)));
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
			
            InternetButton.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            EnterpriseButton.SetBackgroundImage(Images.Buttons.BlackButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            InternetButton.Layer.ShadowColor = UIColor.Black.CGColor;
            InternetButton.Layer.ShadowOffset = new SizeF(0, 1);
            InternetButton.Layer.ShadowOpacity = 0.3f;

            EnterpriseButton.Layer.ShadowColor = UIColor.Black.CGColor;
            EnterpriseButton.Layer.ShadowOffset = new SizeF(0, 1);
            EnterpriseButton.Layer.ShadowOpacity = 0.3f;

            InternetButton.TouchUpInside += InternetButtonClicked;
            EnterpriseButton.TouchUpInside += EnterpriseButtonClicked;

            ScrollView.ContentSize = new SizeF(View.Bounds.Width, EnterpriseButton.Frame.Bottom + 10f);
        }

        void EnterpriseButtonClicked (object sender, EventArgs e)
        {
            NavigationController.PushViewController(new LoginViewController(true), true);
        }

        void InternetButtonClicked (object sender, EventArgs e)
        {
            NavigationController.PushViewController(new LoginViewController(false), true);
        }
    }
}

