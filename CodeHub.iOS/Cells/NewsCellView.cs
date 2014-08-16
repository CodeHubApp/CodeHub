using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using CodeHub.iOS;

namespace CodeHub.iOS.Cells
{
    public partial class NewsCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("NewsCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("NewsCellView");
        public static readonly UIEdgeInsets EdgeInsets = new UIEdgeInsets(0, 48f, 0, 0);

        public static UIFont TimeFont
        {
            get { return UIFont.SystemFontOfSize(12f * Theme.CurrentTheme.FontSizeRatio); }
        }

        public static UIFont HeaderFont
        {
            get { return UIFont.SystemFontOfSize(13f * Theme.CurrentTheme.FontSizeRatio); }
        }

        public static UIFont BodyFont
        {
            get { return UIFont.SystemFontOfSize(13f * Theme.CurrentTheme.FontSizeRatio); }
        }

        public class Link
        {
            public NSRange Range;
            public NSAction Callback;
            public int Id;
        }

        public NewsCellView(IntPtr handle) : base(handle)
        {
        }

        class LabelDelegate : MonoTouch.TTTAttributedLabel.TTTAttributedLabelDelegate {

            private readonly List<Link> _links;
            private readonly Action<NSUrl> _webLinkClicked;

            public LabelDelegate(List<Link> links, Action<NSUrl> webLinkClicked)
            {
                _links = links;
                _webLinkClicked = webLinkClicked;
            }

            public override void DidSelectLinkWithURL (MonoTouch.TTTAttributedLabel.TTTAttributedLabel label, NSUrl url)
            {
                try
                {
                    if (url.ToString().StartsWith("http", StringComparison.Ordinal))
                    {
                        if (_webLinkClicked != null)
                            _webLinkClicked(url);
                    }
                    else
                    {
                        var i = Int32.Parse(url.ToString());
                        _links[i].Callback();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to callback on TTTAttributedLabel: {0}", e.Message);
                }
            }
        }

        public UIImage UserImage
        {
            get { return Image.Image; }
            set
            {
                Image.Image = value;
                Image.SetNeedsDisplay();
            }
        }

        public void Set(UIImage img, string time, UIImage actionImage, 
            NSMutableAttributedString header, NSMutableAttributedString body, 
            List<Link> headerLinks, List<Link> bodyLinks, Action<NSUrl> webLinkClicked)
        {
            this.Image.Image = img;
            this.Time.Text = time;
            this.ActionImage.Image = actionImage;

            if (header == null)
                header = new NSMutableAttributedString();
            if (body == null)
                body = new NSMutableAttributedString();

            this.Header.Text = header;
            this.Header.Delegate = new LabelDelegate(headerLinks, webLinkClicked);

            this.Body.Text = body;
            this.Body.Hidden = body.Length == 0;
            this.Body.Delegate = new LabelDelegate(bodyLinks, webLinkClicked);

            foreach (var b in headerLinks)
                this.Header.AddLinkToURL(new NSUrl(b.Id.ToString()), b.Range);

            foreach (var b in bodyLinks)
                this.Body.AddLinkToURL(new NSUrl(b.Id.ToString()), b.Range);
        }

        public static NewsCellView Create()
        {
            var cell = (NewsCellView)Nib.Instantiate(null, null)[0];
            cell.SeparatorInset = EdgeInsets;
            cell.Body.LinkAttributes = new NSDictionary();
            cell.Body.ActiveLinkAttributes = new NSMutableDictionary();
            cell.Body.ActiveLinkAttributes[MonoTouch.CoreText.CTStringAttributeKey.UnderlineStyle] = NSNumber.FromBoolean(true);
            cell.Body.Lines = 0;
            cell.Body.LineBreakMode = UILineBreakMode.TailTruncation;

            cell.Header.LinkAttributes = new NSDictionary();
            cell.Header.ActiveLinkAttributes = new NSMutableDictionary();
            cell.Header.ActiveLinkAttributes[MonoTouch.CoreText.CTStringAttributeKey.UnderlineStyle] = NSNumber.FromBoolean(true);
            cell.Header.Lines = 2;
            cell.Header.LineBreakMode = UILineBreakMode.TailTruncation;

            // Special for large fonts
            if (Theme.CurrentTheme.FontSizeRatio > 1.0f)
            {
                cell.Header.Font = HeaderFont;
                cell.Body.Font = BodyFont;
                cell.Time.Font = TimeFont;

                var timeSectionheight = (float)Math.Ceiling(TimeFont.LineHeight);
                var timeFrame = cell.Time.Frame;
                timeFrame.Height = timeSectionheight;
                cell.Time.Frame = timeFrame;

                var imageFrame = cell.ActionImage.Frame;
                imageFrame.Y += (timeFrame.Height - imageFrame.Height) / 2f;
                cell.ActionImage.Frame = imageFrame;

                var headerSectionheight = (float)Math.Ceiling(TimeFont.LineHeight);
                var headerFrame = cell.Header.Frame;
                headerFrame.Height = headerSectionheight * 2f + (float)Math.Ceiling(3f * Theme.CurrentTheme.FontSizeRatio);
                headerFrame.Y = 6 + timeFrame.Height + 5f;
                cell.Header.Frame = headerFrame;

                var picFrame = cell.Image.Frame;
                picFrame.Y = 6 + timeFrame.Height + 5f;
                picFrame.Y += (headerFrame.Height - picFrame.Height) / 2f;
                cell.Image.Frame = picFrame;

                var bodyFrame = cell.Body.Frame;
                bodyFrame.Y = headerFrame.Y + headerFrame.Height + 4f;
                cell.Body.Frame = bodyFrame;
            }

            return cell;
        }
    }
}

