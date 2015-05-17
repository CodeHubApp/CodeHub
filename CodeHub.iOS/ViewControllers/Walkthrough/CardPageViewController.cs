using UIKit;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class CardPageViewController : UIViewController
    {
        private readonly UIViewController _viewController;

        public CardPageViewController(UIViewController viewController)
            : base("CardPageViewController", null)
        {
            _viewController = viewController;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddChildViewController(_viewController);
            _viewController.View.Frame = new CoreGraphics.CGRect(0, 0, ContentView.Frame.Width, ContentView.Frame.Height);
            _viewController.View.AutoresizingMask = UIViewAutoresizing.All;

            ContentView.Add(_viewController.View);
            ContentView.Layer.CornerRadius = 12f;
            ContentView.Layer.MasksToBounds = true;
        }
    }
}

