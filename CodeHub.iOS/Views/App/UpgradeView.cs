using System;
using ReactiveUI;
using UIKit;
using CodeHub.WebViews;
using CodeHub.Core.ViewModels.App;
using Foundation;
using CodeHub.iOS.Services;
using System.Threading.Tasks;
using System.Linq;
using CodeHub.Core.Services;
using Splat;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.iOS.Views.App
{
    public class UpgradeView : BaseViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Pro Upgrade";

            var web = new UIWebView {ScalesPageToFit = true, AutoresizingMask = UIViewAutoresizing.All};
            web.LoadFinished += (sender, e) => NetworkActivityService.Instance.PopNetworkActive();
            web.LoadStarted += (sender, e) => NetworkActivityService.Instance.PushNetworkActive();
            web.LoadError += (sender, e) => NetworkActivityService.Instance.PopNetworkActive();
            web.ShouldStartLoad = (w, r, n) => ShouldStartLoad(r, n);
            web.Frame = new CoreGraphics.CGRect(0, 0, View.Frame.Width, View.Frame.Height);
            web.UserInteractionEnabled = false;
            Add(web);

            var activityView = new UIActivityIndicatorView
            {
                Frame = new CoreGraphics.CGRect(0, 44, View.Frame.Width, 88f),
                Color = Theme.PrimaryNavigationBarColor,
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                Alpha = 1
            };
            View.Add(activityView);
            activityView.StartAnimating();

            Load().ToBackground(x => {
                var content = new UpgradeDetailsRazorView { Model = x }.GenerateString();
                web.LoadHtmlString(content, NSBundle.MainBundle.BundleUrl);
                web.UserInteractionEnabled = true;

                UIView.Animate(0.2f, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseInOut,
                    () => activityView.Alpha = 0, () =>
                    {
                        activityView.RemoveFromSuperview();
                        activityView.StopAnimating();
                    });
            });
        }

        private async Task<UpgradeDetailsModel> Load()
        {
            var featureService = Locator.Current.GetService<IFeaturesService>();
            var productData = (await InAppPurchaseService.Instance.RequestProductData(FeatureIds.ProEdition)).Products.FirstOrDefault();
            var enabled = featureService.IsProEnabled;
            return new UpgradeDetailsModel(productData != null ? productData.LocalizedPrice().ToString() : null, enabled);
        }

        protected virtual bool ShouldStartLoad (NSUrlRequest request, UIWebViewNavigationType navigationType)
        {

            var url = request.Url;

            if (url.Scheme.Equals("app"))
            {
                var func = url.Host;

                if (string.Equals(func, "buy", StringComparison.OrdinalIgnoreCase))
                {
                    // Purchase
                }
                else if (string.Equals(func, "restore", StringComparison.OrdinalIgnoreCase))
                {
                    // Restore
                }

                return false;
            }

            if (url.Scheme.Equals("file"))
            {
                return true;
            }

            if (url.Scheme.Equals("http") || url.Scheme.Equals("https"))
            {
                var vm = new WebBrowserViewModel().Init(url.AbsoluteString);
                var view = new WebBrowserView(true, true) { ViewModel = vm };
                view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, 
                    (s, e) => DismissViewController(true, null));
                PresentViewController(new ThemedNavigationController(view), true, null);
                return false;
            }

            return false;
        }
    }
}

