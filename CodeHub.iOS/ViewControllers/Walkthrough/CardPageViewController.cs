using UIKit;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class CardPageViewController : BaseViewController
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

            _viewController.View.Frame = new CoreGraphics.CGRect(0, 0, ContentView.Frame.Width, ContentView.Frame.Height);
            _viewController.View.AutoresizingMask = UIViewAutoresizing.All;
            AddChildViewController(_viewController);

            ContentView.Layer.CornerRadius = 12f;
            ContentView.Layer.MasksToBounds = true;
            ContentView.Add(_viewController.View);
        }
    }
}

