using System;
using CodeHub.Core.Services;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views
{
    public abstract class BaseWebView<TViewModel> : BaseViewController<TViewModel> where TViewModel : class
    {
        private bool _domLoaded;
        private readonly INetworkActivityService _networkActivityService;
        private readonly List<string> _toBeExecuted = new List<string>();

        public UIWebView Web { get; private set; }

        protected BaseWebView(INetworkActivityService networkActivityService)
        {
            _networkActivityService = networkActivityService;

            Web = new UIWebView {ScalesPageToFit = true};
            Web.LoadFinished += OnLoadFinished;
            Web.LoadStarted += OnLoadStarted;
            Web.LoadError += OnLoadError;
            Web.ShouldStartLoad = (w, r, n) => ShouldStartLoad(r, n);
        }

        protected virtual bool ShouldStartLoad (NSUrlRequest request, UIWebViewNavigationType navigationType)
        {

            var url = request.Url;
            if(url.Scheme.Equals("app")) {
                var func = url.Host;

                if (func.Equals("ready"))
                {
                    _domLoaded = true;
                    foreach (var e in _toBeExecuted)
                        Web.EvaluateJavascript(e);
                    _toBeExecuted.Clear();
                }

                return false;
            }

            return true;
        }

        protected virtual void OnLoadError (object sender, UIWebErrorArgs e)
        {
            _networkActivityService.PopNetworkActive();
        }

        protected virtual void OnLoadStarted (object sender, EventArgs e)
        {
            _networkActivityService.PushNetworkActive();
        }

        protected virtual void OnLoadFinished(object sender, EventArgs e)
        {
            _networkActivityService.PopNetworkActive();
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

        protected void LoadFile(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            GoUrl(new NSUrl(uri.AbsoluteUri + "#" + Environment.TickCount));
        }

        protected void LoadContent(string content, string contextPath)
        {
            contextPath = contextPath.Replace("/", "//").Replace(" ", "%20");
            Web.LoadHtmlString(content, NSUrl.FromString("file:/" + contextPath + "//"));
        }

        protected void LoadContent(string content)
        {
            Web.LoadHtmlString(content ?? string.Empty, NSBundle.MainBundle.BundleUrl);
        }

        protected void ExecuteJavascript(string data)
        {
            if (_domLoaded)
                InvokeOnMainThread(() => Web.EvaluateJavascript(data));
            else
                _toBeExecuted.Add(data);
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
            Web.Frame = bounds;
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            Web.Frame = View.Bounds;
        }
    }
}

