using System;
using MonoTouch.Dialog;
using UIKit;
using Foundation;
using CoreGraphics;

namespace CodeHub.iOS.Elements
{
    public class WebElement : Element, IElementSizing
    {
        protected readonly UIWebView WebView = null;
        private nfloat _height;
        private string _value;
        private bool _isLoaded = false;
        protected readonly NSString Key;
        private readonly bool _rawContentLoad;

        public Action<nfloat> HeightChanged;


        public Action<string> UrlRequested;

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
                if (UrlRequested != null)
                    UrlRequested(request.Url.AbsoluteString);
                return false;
            }

            return true;
        }

        public WebElement (string content, string cellKey, bool rawContentLoad) 
            : base (string.Empty)
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
                if (GetImmediateRootElement() != null)
                    GetImmediateRootElement().Reload(this, UITableViewRowAnimation.Fade);
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

        protected override NSString CellKey 
        {
            get { return Key; }
        }

        public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            return _height;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell (CellKey);
            if (cell == null){
                cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
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

