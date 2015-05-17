using UIKit;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class PromoteView : UIViewController
    {
        public PromoteView()
            : base("PromoteView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            StarButton.BackgroundColor = UIColor.FromRGB(0x2c, 0x3e, 0x50);
            StarButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            StarButton.Layer.CornerRadius = 6f;

            WatchButton.BackgroundColor = UIColor.FromRGB(0x7f, 0x8c, 0x8d);
            WatchButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            WatchButton.Layer.CornerRadius = 6f;
        }
    }
}

