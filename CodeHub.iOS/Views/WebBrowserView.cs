using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using ReactiveUI;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Views
{
    public class WebBrowserView : BaseViewController<WebBrowserViewModel>
    {
        private readonly INetworkActivityService _networkActivityService;
        protected UIBarButtonItem BackButton;
        protected UIBarButtonItem RefreshButton;
        protected UIBarButtonItem ForwardButton;

        public UIWebView Web { get; private set; }
        private readonly bool _navigationToolbar;

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

        public WebBrowserView(INetworkActivityService networkActivityService)
            : this(true, true)
        {
            _networkActivityService = networkActivityService;
        }

        public WebBrowserView(bool navigationToolbar, bool showPageAsTitle = false)
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };
            Web = new UIWebView {ScalesPageToFit = true};
            Web.LoadFinished += OnLoadFinished;
            Web.LoadStarted += OnLoadStarted;
            Web.LoadError += OnLoadError;
            Web.ShouldStartLoad = (w, r, n) => ShouldStartLoad(r, n);

            if (showPageAsTitle)
            {
                Web.LoadFinished += (sender, e) =>
                {
                    Title = Web.EvaluateJavascript("document.title");
                };
            }

            _navigationToolbar = navigationToolbar;

            if (_navigationToolbar)
            {
                ToolbarItems = new [] { 
                    (BackButton = new UIBarButtonItem(Images.BackChevron, UIBarButtonItemStyle.Plain, (s, e) => GoBack()) { Enabled = false }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 40f },
                    (ForwardButton = new UIBarButtonItem(Images.ForwardChevron, UIBarButtonItemStyle.Plain, (s, e) => GoForward()) { Enabled = false }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    (RefreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh, (s, e) => Refresh()))
                };

                BackButton.Enabled = false;
                ForwardButton.Enabled = false;
                RefreshButton.Enabled = false;
            }

            EdgesForExtendedLayout = UIRectEdge.None;
        }

        protected virtual bool ShouldStartLoad (NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            return true;
        }

        protected virtual void OnLoadError (object sender, UIWebErrorArgs e)
        {
            _networkActivityService.PopNetworkActive();
            if (RefreshButton != null)
                RefreshButton.Enabled = true;
        }

        protected virtual void OnLoadStarted (object sender, EventArgs e)
        {
            _networkActivityService.PushNetworkActive();
            if (RefreshButton != null)
                RefreshButton.Enabled = false;
        }

        protected virtual void OnLoadFinished(object sender, EventArgs e)
        {
            _networkActivityService.PopNetworkActive();
            if (BackButton != null)
            {
                BackButton.Enabled = Web.CanGoBack;
                ForwardButton.Enabled = Web.CanGoForward;
                RefreshButton.Enabled = true;
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
            Add(Web);
            ViewModel.WhenAnyValue(x => x.Url).SubscribeSafe(x => GoUrl(new NSUrl(x)));
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

        protected void LoadContent(string content, string contextPath)
        {
            contextPath = contextPath.Replace("/", "//").Replace(" ", "%20");
            Web.LoadHtmlString(content, NSUrl.FromString("file:/" + contextPath + "//"));
        }

        public void GoUrl(NSUrl url)
        {
            Web.LoadRequest(new NSUrlRequest(url));
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
            var bounds = View.Bounds;
            if (_navigationToolbar)
                bounds.Height -= NavigationController.Toolbar.Frame.Height;
            Web.Frame = bounds;
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            Web.Frame = View.Bounds;
        }
    }
}

