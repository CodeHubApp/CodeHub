using Foundation;
using UIKit;
using System;

namespace CodeHub.iOS.ViewControllers
{
    public class WebBrowserViewController : WebViewController
    {
        protected UIBarButtonItem BackButton;
        protected UIBarButtonItem RefreshButton;
        protected UIBarButtonItem ForwardButton;

        public WebBrowserViewController()
        {
            BackButton = new UIBarButtonItem { Image = Images.Web.BackButton, Enabled = false };
            ForwardButton = new UIBarButtonItem { Image = Images.Web.FowardButton, Enabled = false };
            RefreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh) { Enabled = false };

            BackButton.TintColor = Theme.CurrentTheme.WebButtonTint;
            ForwardButton.TintColor = Theme.CurrentTheme.WebButtonTint;
            RefreshButton.TintColor = Theme.CurrentTheme.WebButtonTint;

            ToolbarItems = new[]
{
                BackButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 40f },
                ForwardButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                RefreshButton
            };

            OnActivation(d =>
            {
                d(BackButton.GetClickedObservable()
                  .Subscribe(_ => Web.GoBack()));
                
                d(ForwardButton.GetClickedObservable()
                  .Subscribe(_ => Web.GoForward()));
                
                d(RefreshButton.GetClickedObservable()
                  .Subscribe(_ => Web.Reload()));
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft ||
                    UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight)
                {
                    UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Slide);
                }
            }

            var bounds = View.Bounds;
            bounds.Height -= NavigationController.Toolbar.Frame.Height;
            Web.Frame = bounds;

            BackButton.Enabled = Web.CanGoBack;
            ForwardButton.Enabled = Web.CanGoForward;
            RefreshButton.Enabled = !Web.IsLoading;

            NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.SetToolbarHidden(true, animated);
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

        protected override void OnLoadError(NSError error)
        {
            BackButton.Enabled = Web.CanGoBack;
            ForwardButton.Enabled = Web.CanGoForward;
            RefreshButton.Enabled = true;
        }

        protected override void OnLoadStarted(object sender, EventArgs e)
        {
            RefreshButton.Enabled = false;
        }

        protected override void OnLoadFinished(object sender, EventArgs e)
        {
            BackButton.Enabled = Web.CanGoBack;
            ForwardButton.Enabled = Web.CanGoForward;
            RefreshButton.Enabled = true;
        }
    }
}
