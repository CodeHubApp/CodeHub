using System;
using UIKit;
using Foundation;
using CodeHub.iOS.Services;
using System.Threading.Tasks;
using System.Linq;
using CodeHub.Core.Services;
using CodeHub.iOS.ViewControllers;
using BigTed;
using System.Reactive.Disposables;
using CodeHub.iOS.WebViews;
using CodeHub.iOS.Views;
using MvvmCross.Platform;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class UpgradeViewController : WebView
    {
        private readonly IFeaturesService _featuresService = Mvx.Resolve<IFeaturesService>();
        private readonly IInAppPurchaseService _inAppPurchaseService = Mvx.Resolve<IInAppPurchaseService>();
        private UIActivityIndicatorView _activityView;

        public UpgradeViewController() : base(false, false)
        {
            Title = "Pro Upgrade";
            ViewModel = new CodeHub.Core.ViewModels.App.UpgradeViewModel();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _activityView = new UIActivityIndicatorView
            {
                Color = Theme.CurrentTheme.PrimaryColor,
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
            };
            _activityView.Frame = new CoreGraphics.CGRect(0, 44, View.Frame.Width, 88f);

            Load().ToBackground();
        }

        private async Task Load()
        {
            Web.UserInteractionEnabled = false;
            Web.LoadHtmlString("", NSBundle.MainBundle.BundleUrl);

            _activityView.Alpha = 1;
            _activityView.StartAnimating();
            View.Add(_activityView);

            try
            {
                var request = _inAppPurchaseService.RequestProductData(FeaturesService.ProEdition).WithTimeout(TimeSpan.FromSeconds(30));
                var productData = (await request).Products.FirstOrDefault();
                var enabled = _featuresService.IsProEnabled;
                var model = new UpgradeDetailsModel(productData != null ? productData.LocalizedPrice() : null, enabled);
                var content = new UpgradeDetailsRazorView { Model = model }.GenerateString();
                LoadContent(content);
                Web.UserInteractionEnabled = true;
            }
            catch (Exception e)
            {
                AlertDialogService.ShowAlert("Error Loading Upgrades", e.Message);
            }
            finally
            {
                UIView.Animate(0.2f, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseInOut,
                    () => _activityView.Alpha = 0, () =>
                    {
                        _activityView.RemoveFromSuperview();
                        _activityView.StopAnimating();
                    });
            }
        }

        protected override bool ShouldStartLoad(WebKit.WKWebView webView, WebKit.WKNavigationAction navigationAction)
        {
            var url = navigationAction.Request.Url;

            if (url.Scheme.Equals("app"))
            {
                var func = url.Host;

                if (string.Equals(func, "buy", StringComparison.OrdinalIgnoreCase))
                {
                    Activate(_featuresService.ActivatePro).ToBackground();
                }
                else if (string.Equals(func, "restore", StringComparison.OrdinalIgnoreCase))
                {
                    Activate(_featuresService.RestorePro).ToBackground();
                }

                return false;
            }

            if (url.Scheme.Equals("mailto", StringComparison.OrdinalIgnoreCase))
            {
                UIApplication.SharedApplication.OpenUrl(url);
                return false;
            }

            if (url.Scheme.Equals("file"))
            {
                return true;
            }

            if (url.Scheme.Equals("http") || url.Scheme.Equals("https"))
            {
                var view = new WebBrowserViewController(url.AbsoluteString);
                PresentViewController(view, true, null);
                return false;
            }

            return false;
        }

        private async Task Activate(Func<Task> activation)
        {
            try
            {
                BTProgressHUD.ShowContinuousProgress("Activating...", ProgressHUD.MaskType.Gradient);
                using (Disposable.Create(BTProgressHUD.Dismiss))
                    await activation();
                Load().ToBackground();
            }
            catch (Exception e)
            {
                AlertDialogService.ShowAlert("Error", e.Message);
            }
        }
    }

    public static class UpgradeViewControllerExtensions
    {
        public static UIViewController PresentUpgradeViewController(this UIViewController @this)
        {
            var vc = new UpgradeViewController();
            var nav = new ThemedNavigationController(vc);

            var navObj = new UIBarButtonItem(Images.Buttons.CancelButton, UIBarButtonItemStyle.Done, (_, __) => @this.DismissViewController(true, null));
            vc.ViewWillAppearCalled += (sender, e) => vc.NavigationItem.LeftBarButtonItem = navObj;
            vc.ViewDidDisappearCalled += (sender, e) => vc.NavigationItem.LeftBarButtonItem = null;
            @this.PresentViewController(nav, true, null);
            return vc;
        }
    }
}

