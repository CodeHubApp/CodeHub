using System;
using Foundation;
using UIKit;
using System.Collections.Generic;
using CodeHub.iOS;
using SDWebImage;

namespace CodeHub.iOS.TableViewCells
{
    public partial class NewsCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("NewsCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("NewsCellView");

        public class Link
        {
            public NSRange Range;
            public Action Callback;
            public int Id;
        }

        public NewsCellView(IntPtr handle) : base(handle)
        {
        }

        class LabelDelegate : Xamarin.TTTAttributedLabel.TTTAttributedLabelDelegate {

            private readonly List<Link> _links;
            private readonly Action<NSUrl> _webLinkClicked;

            public LabelDelegate(List<Link> links, Action<NSUrl> webLinkClicked)
            {
                _links = links;
                _webLinkClicked = webLinkClicked;
            }

            public override void DidSelectLinkWithURL (Xamarin.TTTAttributedLabel.TTTAttributedLabel label, NSUrl url)
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

        public void Set(Uri imgUrl, string time, UIImage actionImage, 
            NSMutableAttributedString header, NSMutableAttributedString body, 
            List<Link> headerLinks, List<Link> bodyLinks, Action<NSUrl> webLinkClicked, bool multilined)
        {
            if (imgUrl == null)
                Image.Image = Images.Avatar;
            else
                Image.SetImage(new NSUrl(imgUrl.AbsoluteUri), Images.Avatar);
            
            Time.Text = time;
            ActionImage.Image = actionImage;

            if (header == null)
                header = new NSMutableAttributedString();
            if (body == null)
                body = new NSMutableAttributedString();

            Header.AttributedText = header;
            Header.Delegate = new LabelDelegate(headerLinks, webLinkClicked);

            Body.AttributedText = body;
            Body.Hidden = body.Length == 0;
            Body.Lines = multilined ? 0 : 4;
            Body.Delegate = new LabelDelegate(bodyLinks, webLinkClicked);

            foreach (var b in headerLinks)
                Header.AddLinkToURL(new NSUrl(b.Id.ToString()), b.Range);

            foreach (var b in bodyLinks)
                Body.AddLinkToURL(new NSUrl(b.Id.ToString()), b.Range);

            AdjustableConstraint.Constant = Body.Hidden ? 0f : 6f;
        }

        public static NewsCellView Create()
        {
            var linkAttributes = new NSMutableDictionary();
            linkAttributes.Add(UIStringAttributeKey.UnderlineStyle, NSNumber.FromBoolean(true));

            var cell = (NewsCellView)Nib.Instantiate(null, null)[0];
            cell.SeparatorInset = new UIEdgeInsets(0, 48f, 0, 0);
            cell.Body.LinkAttributes = new NSDictionary();
            cell.Body.ActiveLinkAttributes = linkAttributes;
            cell.Body.Lines = 4;
            cell.Body.LineBreakMode = UILineBreakMode.TailTruncation;

            cell.Header.LinkAttributes = new NSDictionary();
            cell.Header.ActiveLinkAttributes = linkAttributes;
            cell.Header.Lines = 2;
            cell.Header.LineBreakMode = UILineBreakMode.TailTruncation;

            cell.Image.Layer.MasksToBounds = true;
            cell.Image.Layer.CornerRadius = cell.Image.Frame.Height / 2;
 
            cell.ActionImage.TintColor = cell.Time.TextColor;

            return cell;
        }
    }
}

