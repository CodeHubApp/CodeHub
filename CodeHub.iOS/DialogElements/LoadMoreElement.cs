//
// This cell does not perform cell recycling, do not use as
// sample code for new elements. 
//
using System;
using CoreGraphics;
using System.Threading;
using CoreFoundation;
using Foundation;
using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class LoadMoreElement : OwnerDrawnElement
    {
        static NSString key = new NSString ("LoadMoreElement");
        public string NormalCaption { get; set; }
        public string LoadingCaption { get; set; }
        public UIColor TextColor { get; set; }
        public UIColor BackgroundColor { get; set; }
        public bool AutoLoadOnVisible { get; set; }
        public event Action<LoadMoreElement> Tapped = null;
        public UIFont Font;
        UITextAlignment alignment = UITextAlignment.Center;
        bool animating;
        UILabel _caption;
        
        public LoadMoreElement () 
            : base (UITableViewCellStyle.Default, key.ToString())
        {
        }
        
        public LoadMoreElement (string normalCaption, string loadingCaption, Action<LoadMoreElement> tapped) : this (normalCaption, loadingCaption, tapped, UIFont.BoldSystemFontOfSize (16), UIColor.Black)
        {
        }
        
        public LoadMoreElement (string normalCaption, string loadingCaption, Action<LoadMoreElement> tapped, UIFont font, UIColor textColor) 
            : base (UITableViewCellStyle.Default, key.ToString())
        {
            NormalCaption = normalCaption;
            LoadingCaption = loadingCaption;
            Tapped += tapped;
            Font = font;
            TextColor = textColor;
        }

        public override void Draw(CGRect bounds, CoreGraphics.CGContext context, UIView view)
        {
            if (AutoLoadOnVisible)
            {
                LoadMore();
            }
        }

        protected override void CellCreated(UITableViewCell cell, UIView view)
        {
            var activityIndicator = new UIActivityIndicatorView () {
                ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray,
                Tag = 1
            };
            _caption = new UILabel () {
                AdjustsFontSizeToFitWidth = false,
                Tag = 2,
                HighlightedTextColor = UIColor.White,
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
            };
            cell.ContentView.AddSubview (_caption);
            cell.ContentView.AddSubview (activityIndicator);
            view.BackgroundColor = BackgroundColor ?? UIColor.Clear;
        }
        
        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = base.GetCell(tv);
            var activityIndicator = cell.ContentView.ViewWithTag (1) as UIActivityIndicatorView;
            var caption = cell.ContentView.ViewWithTag (2) as UILabel;

            if (Animating){
                caption.Text = LoadingCaption;
                activityIndicator.Hidden = false;
                activityIndicator.StartAnimating ();
            } else {
                caption.Text = NormalCaption;
                activityIndicator.Hidden = true;
                activityIndicator.StopAnimating ();
            }
            if (BackgroundColor != null){
                cell.ContentView.BackgroundColor = BackgroundColor ?? UIColor.Clear;
            } else {
                cell.ContentView.BackgroundColor = null;
            }
            caption.BackgroundColor = UIColor.Clear;
            caption.TextColor = TextColor ?? UIColor.Black;
            caption.Font = Font ?? UIFont.BoldSystemFontOfSize (16);
            caption.TextAlignment = Alignment;
            Layout (cell, activityIndicator, caption);
            return cell;
        }
        
        public bool Animating {
            get {
                return animating;
            }
            set {
                if (animating == value)
                    return;
                animating = value;
                var cell = GetActiveCell ();
                if (cell == null)
                    return;
                var activityIndicator = cell.ContentView.ViewWithTag (1) as UIActivityIndicatorView;
                var caption = cell.ContentView.ViewWithTag (2) as UILabel;
                if (value){
                    caption.Text = LoadingCaption;
                    activityIndicator.Hidden = false;
                    activityIndicator.StartAnimating ();
                } else {
                    activityIndicator.StopAnimating ();
                    activityIndicator.Hidden = true;
                    caption.Text = NormalCaption;
                }
                Layout (cell, activityIndicator, caption);
            }
        }
                
        public override void Selected (UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            LoadMore();
        }

        private void LoadMore()
        {
            if (Animating)
                return;

            if (Tapped != null){
                Animating = true;
                Tapped (this);
            }
        }
        
        CGSize GetTextSize (string text)
        {
            return new NSString (text).StringSize (Font, UIScreen.MainScreen.Bounds.Width, UILineBreakMode.TailTruncation);
        }
        
        public static int Padding = 10;
        public static int IndicatorSize = 20;

        public override nfloat Height(CGRect bounds)
        {
            return GetTextSize (Animating ? LoadingCaption : NormalCaption).Height + 2*Padding;
        }
        
        void Layout (UITableViewCell cell, UIActivityIndicatorView activityIndicator, UILabel caption)
        {
            var sbounds = cell.ContentView.Bounds;

            var size = GetTextSize (Animating ? LoadingCaption : NormalCaption);
            
            if (!activityIndicator.Hidden)
                activityIndicator.Frame = new CGRect ((sbounds.Width-size.Width)/2-IndicatorSize*2, Padding, IndicatorSize, IndicatorSize);

            caption.Frame = new CGRect (10, Padding, sbounds.Width-20, size.Height);
        }
        
        public UITextAlignment Alignment { 
            get { return alignment; } 
            set { alignment = value; }
        }
        public UITableViewCellAccessory Accessory { get; set; }
    }
}

