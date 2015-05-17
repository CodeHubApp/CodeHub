using UIKit;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class WelcomeViewController : UIViewController
    {
        public WelcomeViewController()
            : base("WelcomeViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            GoButton.BackgroundColor = UIColor.FromRGB(0x29, 0x80, 0xb9);
            GoButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            GoButton.Layer.CornerRadius = 6f;
        }
    }
}

