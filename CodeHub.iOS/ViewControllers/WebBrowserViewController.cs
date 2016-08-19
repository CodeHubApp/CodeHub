using CodeHub.Core.ViewModels;
using CodeHub.iOS.Services;
using Foundation;
using MvvmCross.iOS.Views;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class WebBrowserViewController : MvxViewController, IMvxModalIosView
    {
        void HandleAction()
        {

        }

        private UIStatusBarStyle _statusBarStyle;

        public WebBrowserViewController()
        {
        }

        public WebBrowserViewController(string url)
        {
            var vm = new WebBrowserViewModel();
            vm.Init(new WebBrowserViewModel.NavObject { Url = url });
            ViewModel = vm;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            _statusBarStyle = UIApplication.SharedApplication.StatusBarStyle;
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.Default, animated);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft ||
                    UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight)
                {
                    UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Slide);
                }
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            UIApplication.SharedApplication.SetStatusBarStyle(_statusBarStyle, animated);
            UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Slide);
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillRotate(toInterfaceOrientation, duration);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {

                if (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft ||
                    toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
                {
                    UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Slide);
                }
                else
                {
                    UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Slide);
                }
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var vm = ViewModel as WebBrowserViewModel;

            System.Uri uri;
            if (!System.Uri.TryCreate(vm.Url, System.UriKind.Absolute, out uri))
            {
                AlertDialogService.ShowAlert("Error", "Unable to display webpage as the provided link (" + vm.Url + ") was invalid");
            }
            else
            {
                var safariBrowser = new SafariServices.SFSafariViewController(new NSUrl(vm.Url));
                safariBrowser.View.Frame = View.Bounds;
                safariBrowser.View.AutoresizingMask = UIViewAutoresizing.All;
                AddChildViewController(safariBrowser);
                Add(safariBrowser.View);
            }
        }
    }
}
