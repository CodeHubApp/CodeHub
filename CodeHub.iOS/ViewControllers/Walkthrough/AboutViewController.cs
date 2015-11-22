namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class AboutViewController : BaseViewController
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

