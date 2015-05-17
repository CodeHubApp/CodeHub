using UIKit;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class AboutViewController : UIViewController
    {
        public AboutViewController()
            : base("AboutViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            IconImageView.Layer.CornerRadius = 24f;
            IconImageView.Layer.MasksToBounds = true;
        }
    }
}

