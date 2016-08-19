using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using Foundation;
using UIKit;
using CodeHub.iOS.TableViewCells;
using Humanizer;

namespace CodeHub.iOS.DialogElements
{
    public class NewsFeedElement : Element
    {
        private readonly string _time;
        private readonly Uri _imageUri;
        private readonly UIImage _actionImage;
        private readonly Action _tapped;
        private readonly bool _multilined;

        private readonly NSMutableAttributedString _attributedHeader;
        private readonly NSMutableAttributedString _attributedBody;
        private readonly List<NewsCellView.Link> _headerLinks;
        private readonly List<NewsCellView.Link> _bodyLinks;

        public static UIColor LinkColor = Theme.CurrentTheme.MainTitleColor;

        private UIImage LittleImage { get; set; }

        public Action<NSUrl> WebLinkClicked;

        public class TextBlock
        {
            public string Value;
            public Action Tapped;

            public TextBlock()
            {
            }

            public TextBlock(string value)
            {
                Value = value;
            }

            public TextBlock(string value, Action tapped = null)
                : this (value)
            {
                Tapped = tapped;
            }
        }

        public NewsFeedElement(string imageUrl, DateTimeOffset time, IEnumerable<TextBlock> headerBlocks, IEnumerable<TextBlock> bodyBlocks, UIImage littleImage, Action tapped, bool multilined)
        {
            Uri.TryCreate(imageUrl, UriKind.Absolute, out _imageUri);
            _time = time.Humanize();
            _actionImage = littleImage;
            _tapped = tapped;
            _multilined = multilined;

            var header = CreateAttributedStringFromBlocks(UIFont.PreferredBody, Theme.CurrentTheme.MainTextColor, headerBlocks);
            _attributedHeader = header.Item1;
            _headerLinks = header.Item2;

            var body = CreateAttributedStringFromBlocks(UIFont.PreferredSubheadline, Theme.CurrentTheme.MainSubtitleColor, bodyBlocks);
            _attributedBody = body.Item1;
            _bodyLinks = body.Item2;
        }

        private static Tuple<NSMutableAttributedString,List<NewsCellView.Link>> CreateAttributedStringFromBlocks(UIFont font, UIColor primaryColor, IEnumerable<TextBlock> blocks)
        {
            var attributedString = new NSMutableAttributedString();
            var links = new List<NewsCellView.Link>();

            nint lengthCounter = 0;
            int i = 0;

            foreach (var b in blocks)
            {
                UIColor color = null;
                if (b.Tapped != null)
                    color = LinkColor;

                color = color ?? primaryColor; 

                var ctFont = new CoreText.CTFont(font.Name, font.PointSize);
                var str = new NSAttributedString(b.Value, new CoreText.CTStringAttributes() { ForegroundColor = color.CGColor, Font = ctFont });
                attributedString.Append(str);
                var strLength = str.Length;

                if (b.Tapped != null)
                {
                    var weakTapped = new WeakReference<Action>(b.Tapped);
                    links.Add(new NewsCellView.Link { Range = new NSRange(lengthCounter, strLength), Callback = () => weakTapped.Get()?.Invoke(), Id = i++ });
                }

                lengthCounter += strLength;
            }

            return new Tuple<NSMutableAttributedString, List<NewsCellView.Link>>(attributedString, links);
        }


        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(NewsCellView.Key) as NewsCellView ?? NewsCellView.Create();
            cell.Set(_imageUri, _time, _actionImage, _attributedHeader, _attributedBody, _headerLinks, _bodyLinks, WebLinkClicked, _multilined);
            return cell;
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            _tapped?.Invoke();
        }
    }
}