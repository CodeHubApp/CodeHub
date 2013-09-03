using System;
using MonoTouch.UIKit;
using RedPlum;
using System.Threading;
using MonoTouch;
using CodeHub.Data;
using CodeFramework.Views;
using System.Linq;

namespace CodeHub.ViewControllers
{
    public partial class LoginViewController : UIViewController
    {
        public Action<string, string> Login;

        private string _username;

        public string Username {
            get { return _username; }
            set {
                _username = value;
                if (User != null)
                    User.Text = _username;
            }
        }

        public LoginViewController()
            : base("LoginViewController", null)
        {
            Title = "Login".t();
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.BackButton, () => NavigationController.PopViewControllerAnimated(true)));
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            try
            {
                var backgroundImage = CreateRepeatingBackground();
                if (backgroundImage != null)
                {
                    View.BackgroundColor = UIColor.FromPatternImage(backgroundImage);
                    backgroundImage.Dispose();
                }
            }
            catch (Exception e)
            {
                Utilities.Analytics.TrackException(false, e.Message);
            }
        }

        private UIImage CreateRepeatingBackground()
        {
            UIImage bgImage = null;
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                bgImage = UIImageHelper.FromFileAuto(MonoTouch.Utilities.IsTall ? "Default-568h" : "Default");
            }
            else
            {
                if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.Portrait || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.PortraitUpsideDown)
                    bgImage = UIImageHelper.FromFileAuto("Default-Portrait");
                else
                    bgImage = UIImageHelper.FromFileAuto("Default-Landscape");
            }

            if (bgImage == null)
                return null;

            UIGraphics.BeginImageContext(new System.Drawing.SizeF(40f, bgImage.Size.Height));
            var ctx = UIGraphics.GetCurrentContext();
            ctx.TranslateCTM (0, bgImage.Size.Height);
            ctx.ScaleCTM (1f, -1f);

            ctx.DrawImage(new System.Drawing.RectangleF(-10, 0, bgImage.Size.Width, bgImage.Size.Height), bgImage.CGImage);

            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            bgImage.Dispose();
            bgImage = null;

            return img;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Logo.Image = Images.Logos.GitHub;
            if (Username != null)
                User.Text = Username;

            User.ShouldReturn = delegate {
                Password.BecomeFirstResponder();
                return true;
            };
            Password.ShouldReturn = delegate {
                Password.ResignFirstResponder();

                //Run this in another thread
                Login(User.Text, Password.Text);
                return true;
            };
        }

        [Obsolete("Deprecated in iOS 6.0")]
        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            // Clear any references to subviews of the main view in order to
            // allow the Garbage Collector to collect them sooner.
            //
            // e.g. myOutlet.Dispose (); myOutlet = null;

            ReleaseDesignerOutlets();
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
            return UIInterfaceOrientationMask.All;
        }

        [Obsolete]
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                if (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
                    return true;
                return false;
            }
            return true;
        }
    }
}

