using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace CodeHub.iOS.DialogElements
{
    public class HtmlElement : Element, IElementSizing, IDisposable
    {
        protected UIWebView WebView;
        private float _height;
        protected readonly NSString Key;

        public Action<float> HeightChanged;

        public Action<string> UrlRequested;

        public float Height
        {
            get { return _height; }
        }

        public bool HasValue { get; private set; }

        public string ContentPath
        {
            set
            {
                if (value == null)
                {
                    HasValue = false;
                    WebView.LoadHtmlString(string.Empty, NSBundle.MainBundle.BundleUrl);
                }
                else
                {
                    HasValue = true;
                    WebView.LoadRequest(new NSUrlRequest(NSUrl.FromFilename(value)));
                }
            }
        }

        public string Value
        {
            set
            {
                if (value == null)
                {
                    HasValue = false;
                    WebView.LoadHtmlString(string.Empty, NSBundle.MainBundle.BundleUrl);
                }
                else
                {
                    HasValue = true;
                    WebView.LoadHtmlString(value, NSBundle.MainBundle.BundleUrl);
                }
            }
        }

        private bool ShouldStartLoad (NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("app://resize"))
            {
//                try
//                {
//                    _height = WebView.SizeThatFits(SizeF.Empty).Height;
//                    if (HeightChanged != null)
//                        HeightChanged(_height);
//                }
//                catch
//                {
//                }

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

        public void Dispose()
        {
            WebView.RemoveFromSuperview();
            WebView.Dispose();
            WebView = null;
        }

        public HtmlElement (string cellKey) 
        {
            Key = new NSString(cellKey);
            WebView = new UIWebView();
            WebView.ScrollView.ScrollEnabled = false;
            WebView.ScalesPageToFit = true;
            WebView.ScrollView.Bounces = false;
            WebView.ShouldStartLoad = (w, r, n) => ShouldStartLoad(r, n);
            WebView.LoadFinished += (sender, e) => 
            {
                _height = WebView.SizeThatFits(SizeF.Empty).Height;
                if (HeightChanged != null)
                    HeightChanged(_height);
            };

            HeightChanged = (x) => {
                var root = this.GetRootElement();
                if (root != null)
                    root.Reload(this);
            };
        }

        public float GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            return HasValue ? _height : 100f;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell (Key);
            if (cell == null){
                cell = new UITableViewCell (UITableViewCellStyle.Default, Key);
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            }  

            WebView.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width, cell.ContentView.Frame.Height);
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

