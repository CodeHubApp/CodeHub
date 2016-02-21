using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CodeHub.iOS.DialogElements
{
    public class WebElement : Element, IElementSizing
    {
        protected readonly UIWebView WebView = null;
        private nfloat _height;
        private string _value;
        private bool _isLoaded = false;
        protected readonly NSString Key;
        private readonly bool _rawContentLoad;
        private readonly Subject<string> _urlSubject = new Subject<string>();

        public Action<nfloat> HeightChanged;

        public IObservable<string> UrlRequested
        {
            get { return _urlSubject.AsObservable(); }
        }

        public nfloat Height
        {
            get { return _height; }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_isLoaded)
                    LoadContent(value);
            }
        }

        private bool ShouldStartLoad (NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("app://resize"))
            {
                try
                {
                    var size = WebView.EvaluateJavascript("size();");
                    if (size != null)
                        nfloat.TryParse(size, out _height);

                    if (HeightChanged != null)
                        HeightChanged(_height);
                }
                catch
                {
                }

                return false;
            }

            if (!request.Url.AbsoluteString.StartsWith("file://"))
            {
                _urlSubject.OnNext(request.Url.AbsoluteString);
                return false;
            }

            return true;
        }

        public WebElement (string content, string cellKey, bool rawContentLoad) 
        {
            Key = new NSString(cellKey);
            _rawContentLoad = rawContentLoad;
            WebView = new UIWebView();
            WebView.ScrollView.ScrollEnabled = false;
            WebView.ScrollView.Bounces = false;
            WebView.ShouldStartLoad = (w, r, n) => ShouldStartLoad(r, n);
            WebView.LoadFinished += (sender, e) => {
                if (!string.IsNullOrEmpty(_value))
                    LoadContent(_value);
                _isLoaded = true;
            };

            WebView.LoadHtmlString(content, new NSUrl(""));
            HeightChanged = (x) => {
                GetRootElement()?.Reload(this);
            };
        }

        private void LoadContent(string content)
        {
            if (_rawContentLoad)
            {
                WebView.EvaluateJavascript("var a = " + content + "; ins(a);");
            }
            else
            {
                content = System.Web.HttpUtility.JavaScriptStringEncode(content);
                WebView.EvaluateJavascript("ins('" + content + "');");
            }
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
                WebView.AutoresizingMask = UIViewAutoresizing.All;
            }  

            WebView.Frame = new CGRect(0, 0, cell.ContentView.Frame.Width, cell.ContentView.Frame.Height);
            WebView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            WebView.RemoveFromSuperview();
            cell.ContentView.AddSubview (WebView);
            cell.ContentView.Layer.MasksToBounds = true;
            cell.ContentView.AutosizesSubviews = true;
            cell.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);
            return cell;
        }
    }
}

