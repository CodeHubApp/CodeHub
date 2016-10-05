using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using WebKit;
using System.Threading.Tasks;

namespace CodeHub.iOS.DialogElements
{
    public class HtmlElement : Element, IElementSizing, IDisposable
    {
        private readonly ISubject<string> _urlSubject = new Subject<string>();
        private readonly Lazy<WKWebView> _webView;
        private nfloat _height;
        protected readonly NSString Key;

        public IObservable<string> UrlRequested
        {
            get { return _urlSubject.AsObservable(); }
        }

        private WKWebView WebView
        {
            get { return _webView.Value; }
        }

        public bool HasValue { get; private set; }

        public nfloat Height
        {
            get { return _height; }
        }

        public void SetValue(string value)
        {
            if (value == null)
            {
                WebView.LoadHtmlString(string.Empty, NSBundle.MainBundle.BundleUrl);
            }
            else
            {
                WebView.LoadHtmlString(value, NSBundle.MainBundle.BundleUrl);
            }

            HasValue = value != null;
        }

        public void SetLayout()
        {
            WebView.SetNeedsLayout();
        }

        private async Task<nfloat> GetSize()
        {
            if (HasValue)
            {
                try
                {
                    var size = await WebView.EvaluateJavaScriptAsync("size();");
                    if (size != null)
                    {
                        nfloat newsize;
                        if (nfloat.TryParse(size.ToString(), out newsize))
                            return newsize;
                    }
                }
                catch
                {
                }
            }

            return _height;
        }

        private async Task RefreshSize()
        {
            var size = await GetSize();
            if (_height != size)
            {
                _height = size;
                Reload();
            }
        }

        public void Dispose()
        {
            WebView.RemoveFromSuperview();
            WebView.Dispose();
        }

        public async Task ForceResize()
        {
            var f = WebView.Frame;
            f.Height = 1;
            WebView.Frame = f;
            f.Height = _height = await GetSize();
            WebView.Frame = f;
        }

        public HtmlElement (string cellKey) 
        {
            Key = new NSString(cellKey);
            _height = 0f;

            _webView = new Lazy<WKWebView>(() =>
            {
                var bounds = UIScreen.MainScreen.Bounds;
                var webView = new WKWebView(new CGRect(0, 0, bounds.Width, 44f), new WKWebViewConfiguration());
                webView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
                webView.ScrollView.ScrollEnabled = false;
                webView.ScrollView.Bounces = false;
                webView.NavigationDelegate = new NavigationDelegate(this);
                return webView;
            });
        }

        public void CheckHeight()
        {
//            var f = WebView.Frame;
//            f.Height = 1;
//            WebView.Frame = f;
//            f.Height = _height = await GetSize();
//            WebView.Frame = f;
            Reload();
        }

        public void Reload()
        {
            GetRootElement()?.Reload(this);
        }

        public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            return _height;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell (Key);
            if (cell == null){
                cell = new UITableViewCell (UITableViewCellStyle.Default, Key);
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            }  

            WebView.Frame = new CGRect(0, 0, cell.ContentView.Frame.Width, cell.ContentView.Frame.Height);
            WebView.RemoveFromSuperview();
            cell.ContentView.AddSubview (WebView);
            cell.ContentView.Layer.MasksToBounds = true;
            cell.ContentView.AutosizesSubviews = true;
            cell.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);
            return cell;
        }

        private void OnLoadFinished()
        {
            RefreshSize().ToBackground();
        }

        private bool OnLoadStart(WKNavigationAction navigation)
        {
            if (navigation.Request.Url.AbsoluteString.StartsWith("app://resize"))
            {
                RefreshSize().ToBackground();
                return false;
            }

            if (!navigation.Request.Url.AbsoluteString.StartsWith("file://"))
            {
                if (UrlRequested != null)
                    _urlSubject.OnNext(navigation.Request.Url.AbsoluteString);
                return false;
            }

            return true;
        }

        private class NavigationDelegate : WKNavigationDelegate
        {
            private readonly WeakReference<HtmlElement> _parent;

            public NavigationDelegate(HtmlElement parent)
            {
                _parent = new WeakReference<HtmlElement>(parent);
            }

            public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
            {
                _parent.Get()?.OnLoadFinished();
            }

            public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
            {
                var ret = _parent.Get()?.OnLoadStart(navigationAction) ?? true;
                decisionHandler(ret ? WKNavigationActionPolicy.Allow : WKNavigationActionPolicy.Cancel);
            }
        }
    }
}

