using Foundation;
using SafariServices;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class SafariViewController : SFSafariViewController
    {
        private UIStatusBarStyle _previousStatusBarStyle;

        public SafariViewController(NSUrl url)
            : base(url)
        {
            ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            _previousStatusBarStyle = UIApplication.SharedApplication.StatusBarStyle;
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.Default, true);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            UIApplication.SharedApplication.SetStatusBarStyle(_previousStatusBarStyle, true);
        }
    }
}
