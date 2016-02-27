// Analysis disable once CheckNamespace
namespace UIKit
{
    public static class UIWindowExtensions
    {
        public static UIViewController GetVisibleViewController(this UIWindow @this)
        {
            var topViewController = @this.RootViewController;
            while (topViewController.PresentedViewController != null)
                topViewController = topViewController.PresentedViewController;
            return topViewController;
        }
    }
}

