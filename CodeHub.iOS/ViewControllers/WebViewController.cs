using System;
using CodeHub.iOS.Utilities;
using Foundation;
using UIKit;
using WebKit;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class WebViewController : BaseViewController
    {
        private bool _networkActivity;

        public WKWebView Web { get; }

        protected WebViewController()
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };

            EdgesForExtendedLayout = UIRectEdge.None;

            Web = new WKWebView(CoreGraphics.CGRect.Empty, new WKWebViewConfiguration())
            {
                NavigationDelegate = new NavigationDelegate(this)
            };
        }

        private class NavigationDelegate : WKNavigationDelegate
        {
            private readonly WeakReference<WebViewController> _webView;

            public NavigationDelegate(WebViewController webView)
            {
                _webView = new WeakReference<WebViewController>(webView);
            }

            public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
            {
                _webView.Get()?.OnLoadFinished(null, EventArgs.Empty);
            }

            public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
            {
                _webView.Get()?.OnLoadStarted(null, EventArgs.Empty);
            }

            public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
            {
                _webView.Get()?.OnLoadError(error);
            }

            public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
            {
                var ret = _webView.Get()?.ShouldStartLoad(webView, navigationAction) ?? true;
                decisionHandler(ret ? WKNavigationActionPolicy.Allow : WKNavigationActionPolicy.Cancel);
            }
        }

        protected virtual bool ShouldStartLoad (WKWebView webView, WKNavigationAction navigationAction)
        {
            return true;
        }

        private void ActivateLoadingIndicator()
        {
            if (!_networkActivity)
                NetworkActivity.PushNetworkActive();
            _networkActivity = true;
        }

        private void DeactivateLoadingIndicator()
        {
            if (_networkActivity)
                NetworkActivity.PopNetworkActive();
            _networkActivity = false;
        }

        protected virtual void OnLoadError (NSError error)
        {
        }

        protected virtual void OnLoadStarted (object sender, EventArgs e)
        {
            ActivateLoadingIndicator();
        }

        protected virtual void OnLoadFinished(object sender, EventArgs e)
        {
            DeactivateLoadingIndicator();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Add(Web);
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            Web.Frame = View.Bounds;
        }

        protected static string UrlDecode(string data)
        {
            return System.Web.HttpUtility.UrlDecode(data);
        }

        public string LoadFile(string path)
        {
            if (path == null)
                return string.Empty;

            var uri = Uri.EscapeUriString("file://" + path) + "#" + Environment.TickCount;
            InvokeOnMainThread(() => Web.LoadRequest(new NSUrlRequest(new NSUrl(uri))));
            return uri;
        }

        public void LoadContent(string content)
        {
            Web.LoadHtmlString(content, NSBundle.MainBundle.BundleUrl);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            Web.Frame = View.Bounds;
        }
    }
}

