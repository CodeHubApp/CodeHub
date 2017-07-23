using System;
using UIKit;
using Foundation;
using CodeHub.iOS.Services;
using System.Threading.Tasks;
using System.Linq;
using CodeHub.Core.Services;
using BigTed;
using System.Reactive.Disposables;
using CodeHub.WebViews;
using Splat;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class UpgradeViewController : BaseWebViewController
    {
        private readonly IFeaturesService _featuresService 
            = Locator.Current.GetService<IFeaturesService>();
        private readonly IInAppPurchaseService _inAppPurchaseService
            = Locator.Current.GetService<IInAppPurchaseService>();
        private UIActivityIndicatorView _activityView;

        public UpgradeViewController() : base(false, false)
        {
            Title = "Pro Upgrade";
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
                var response = await _inAppPurchaseService
                    .RequestProductData(FeaturesService.ProEdition)
                    .WithTimeout(TimeSpan.FromSeconds(30));
                
                var productData = response.Products.FirstOrDefault();
                var enabled = _featuresService.IsProEnabled;
                var model = new UpgradeDetailsModel(productData?.LocalizedPrice(), enabled);
                var viewModel = new UpgradeDetailsWebView { Model = model };
                LoadContent(viewModel.GenerateString());
            }
            catch (Exception e)
            {
                AlertDialogService.ShowAlert("Error Loading Upgrades", e.Message);
            }
            finally
            {
                Web.UserInteractionEnabled = true;

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

        public static UpgradeViewController Present(UIViewController parent)
        {
            var vc = new UpgradeViewController();
            var nav = new ThemedNavigationController(vc);

            var navObj = new UIBarButtonItem(
                UIBarButtonSystemItem.Cancel,
                (_, __) => parent.DismissViewController(true, null));

            vc.Appearing.Subscribe(_ => vc.NavigationItem.LeftBarButtonItem = navObj);
            vc.Disappeared.Subscribe(_ => vc.NavigationItem.LeftBarButtonItem = null);

            parent.PresentViewController(nav, true, null);
            return vc;
        }
    }
}

