using System;
using UIKit;
using Foundation;
using WebKit;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class BaseWebViewController : BaseViewController
    {
        protected UIBarButtonItem BackButton;
        protected UIBarButtonItem RefreshButton;
        protected UIBarButtonItem ForwardButton;

        public WKWebView Web { get; private set; }
        private readonly bool _navigationToolbar;
        private readonly bool _showPageAsTitle;
        private bool _networkActivity;
   
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

        protected BaseWebViewController()
            : this(true, true)
        {
        }

        protected BaseWebViewController(bool navigationToolbar, bool showPageAsTitle = false)
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };

            _navigationToolbar = navigationToolbar;
            _showPageAsTitle = showPageAsTitle;

            if (_navigationToolbar)
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

            EdgesForExtendedLayout = UIRectEdge.None;
        }

        private class NavigationDelegate : WKNavigationDelegate
        {
            private readonly WeakReference<BaseWebViewController> _webView;

            public NavigationDelegate(BaseWebViewController webView)
            {
                _webView = new WeakReference<BaseWebViewController>(webView);
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
            if (BackButton != null)
            {
                BackButton.Enabled = Web.CanGoBack;
                ForwardButton.Enabled = Web.CanGoForward;
                RefreshButton.Enabled = true;
            }
        }

        protected virtual void OnLoadStarted (object sender, EventArgs e)
        {
            ActivateLoadingIndicator();

            if (RefreshButton != null)
                RefreshButton.Enabled = false;
        }

        protected virtual void OnLoadFinished(object sender, EventArgs e)
        {
            DeactivateLoadingIndicator();

            if (BackButton != null)
            {
                BackButton.Enabled = Web.CanGoBack;
                ForwardButton.Enabled = Web.CanGoForward;
                RefreshButton.Enabled = true;
            }

            if (_showPageAsTitle)
            {
                Web.EvaluateJavaScript("document.title", (o, _) => {
                    Title = o as NSString;
                });
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Web = new WKWebView(View.Bounds, new WKWebViewConfiguration());
            Web.NavigationDelegate = new NavigationDelegate(this);
            Add(Web);
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            Web.Frame = View.Bounds;
        }

        protected static string JavaScriptStringEncode(string data)
        {
            return System.Web.HttpUtility.JavaScriptStringEncode(data);
        }

        protected static string UrlDecode(string data)
        {
            return System.Web.HttpUtility.UrlDecode(data);
        }

        protected string LoadFile(string path)
        {
            if (path == null)
                return string.Empty;

            var uri = Uri.EscapeUriString("file://" + path) + "#" + Environment.TickCount;
            InvokeOnMainThread(() => Web.LoadRequest(new NSUrlRequest(new NSUrl(uri))));
            return uri;
        }

        protected void LoadContent(string content)
        {
            Web.LoadHtmlString(content, NSBundle.MainBundle.BundleUrl);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var bounds = View.Bounds;
            if (_navigationToolbar)
                bounds.Height -= NavigationController.Toolbar.Frame.Height;
            Web.Frame = bounds;

            if (_navigationToolbar)
            {
                ToolbarItems = new []
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
            }   

            if (_showPageAsTitle)
            {
                Web.EvaluateJavaScript("document.title", (o, _) => {
                    Title = o as NSString;
                });
            }

            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (_navigationToolbar)
                ToolbarItems = null;
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            Web.Frame = View.Bounds;
        }
    }
}

