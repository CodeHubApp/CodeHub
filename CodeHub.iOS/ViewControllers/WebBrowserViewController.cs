using CodeHub.iOS.Services;
using Foundation;
using UIKit;
using System;

namespace CodeHub.iOS.ViewControllers
{
    public class WebBrowserViewController : WebViewController
    {
        private readonly string _url;
        private UIStatusBarStyle _statusBarStyle;

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

            OnActivation(d =>
            {
                d(BackButton.GetClickedObservable().Subscribe(_ => GoBack()));
                d(ForwardButton.GetClickedObservable().Subscribe(_ => GoForward()));
                d(RefreshButton.GetClickedObservable().Subscribe(_ => Refresh()));
            });
        }

        protected virtual void GoBack()
        {
            Web.GoBack();
        }

        protected virtual void Refresh()
        {
            Web.Reload();
        }

        protected virtual void GoForward()
        {
            Web.GoForward();
        }

        public WebBrowserViewController(string url) : this()
        {
            _url = url;
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

            var bounds = View.Bounds;
            bounds.Height -= NavigationController.Toolbar.Frame.Height;
            Web.Frame = bounds;

            ToolbarItems = new[]
            {
                BackButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 40f },
                ForwardButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                RefreshButton
            };

            BackButton.Enabled = Web.CanGoBack;
            ForwardButton.Enabled = Web.CanGoForward;
            RefreshButton.Enabled = !Web.IsLoading;

            Web.EvaluateJavaScript("document.title", (o, _) => Title = o as NSString);

            NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            ToolbarItems = null;
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

            Web.EvaluateJavaScript("document.title", (o, _) => {
                Title = o as NSString;
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            System.Uri uri;
            if (!System.Uri.TryCreate(_url, System.UriKind.Absolute, out uri))
            {
                AlertDialogService.ShowAlert("Error", "Unable to display webpage as the provided link (" + _url + ") was invalid");
            }
            else
            {
                var safariBrowser = new SafariServices.SFSafariViewController(new NSUrl(_url), new SafariServices.SFSafariViewControllerConfiguration {
                    
                });
                safariBrowser.View.Frame = View.Bounds;
                safariBrowser.View.AutoresizingMask = UIViewAutoresizing.All;
                AddChildViewController(safariBrowser);
                Add(safariBrowser.View);
            }
        }
    }
}
