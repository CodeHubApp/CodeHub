using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace CodeHub.iOS.DialogElements
{
    public class WebElement : Element, IElementSizing, IDisposable
    {
        protected UIWebView WebView;
        private float _height;
        protected readonly NSString Key;

        public Action<float> HeightChanged;

        public Action<string> UrlRequested;

        public Action LoadFinished;

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
                try
                {
                    var size = WebView.EvaluateJavascript("size();");
                    if (size != null)
                        float.TryParse(size, out _height);

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

        public void Dispose()
        {
            WebView.RemoveFromSuperview();
            WebView.Dispose();
            WebView = null;
        }

        public WebElement (string cellKey) 
        {
            Key = new NSString(cellKey);
            WebView = new UIWebView();
            WebView.ScrollView.ScrollEnabled = false;
            WebView.ScrollView.Bounces = false;
            WebView.ShouldStartLoad = (w, r, n) => ShouldStartLoad(r, n);
            WebView.LoadFinished += (sender, e) => 
            {
                if (LoadFinished != null && HasValue)
                    LoadFinished();
            };

            HeightChanged = (x) => {
                if (Section != null || Section.Root != null)
                    Section.Root.Reload(this, UITableViewRowAnimation.None);
            };
        }

        public float GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            return HasValue ? _height : 0f;
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

