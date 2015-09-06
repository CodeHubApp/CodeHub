using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using CoreGraphics;
using MonoTouch.Dialog;
using Foundation;
using UIKit;
using MonoTouch.Dialog.Utilities;

namespace CodeFramework.iOS.Elements
{
    public class NewsFeedElement : Element, IElementSizing, IColorizeBackground, IImageUpdated
    {
        private readonly string _time;
        private readonly Uri _imageUri;
        private readonly UIImage _actionImage;
        private readonly int _bodyBlocks;
        private readonly Action _tapped;

        private readonly NSMutableAttributedString _attributedHeader;
        private readonly NSMutableAttributedString _attributedBody;
        private readonly List<NewsCellView.Link> _headerLinks;
        private readonly List<NewsCellView.Link> _bodyLinks;

        public static UIColor LinkColor = Theme.CurrentTheme.MainTitleColor;
        public static UIFont LinkFont = UIFont.BoldSystemFontOfSize(13f);

        private UIImage LittleImage { get; set; }

        public Action<NSUrl> WebLinkClicked;

        public class TextBlock
        {
            public string Value;
            public Action Tapped;
            public UIFont Font;
            public UIColor Color;

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

            public TextBlock(string value, UIFont font = null, UIColor color = null, Action tapped = null)
                : this(value, tapped)
            {
                Font = font; 
                Color = color;
            }
        }

        public NewsFeedElement(string imageUrl, DateTimeOffset time, IEnumerable<TextBlock> headerBlocks, IEnumerable<TextBlock> bodyBlocks, UIImage littleImage, Action tapped)
            : base(null)
        {
            Uri.TryCreate(imageUrl, UriKind.Absolute, out _imageUri);
            _time = time.ToDaysAgo();
            _actionImage = littleImage;
            _tapped = tapped;

            var header = CreateAttributedStringFromBlocks(headerBlocks);
            _attributedHeader = header.Item1;
            _headerLinks = header.Item2;

            var body = CreateAttributedStringFromBlocks(bodyBlocks);
            _attributedBody = body.Item1;
            _bodyLinks = body.Item2;
            _bodyBlocks = bodyBlocks.Count();
        }

        private Tuple<NSMutableAttributedString,List<NewsCellView.Link>> CreateAttributedStringFromBlocks(IEnumerable<TextBlock> blocks)
        {
            var attributedString = new NSMutableAttributedString();
            var links = new List<NewsCellView.Link>();

            nint lengthCounter = 0;
            int i = 0;

            foreach (var b in blocks)
            {
                UIColor color = null;
                if (b.Color != null)
                    color = b.Color;
                else
                {
                    if (b.Tapped != null)
                        color = LinkColor;
                }

                UIFont font = null;
                if (b.Font != null)
                    font = b.Font.WithSize(b.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);
                else
                {
                    if (b.Tapped != null)
                        font = LinkFont.WithSize(LinkFont.PointSize * Theme.CurrentTheme.FontSizeRatio);
                }

                if (color == null)
                    color = Theme.CurrentTheme.MainTextColor;
                if (font == null)
                    font = NewsCellView.BodyFont;


                var ctFont = new CoreText.CTFont(font.Name, font.PointSize);
                var str = new NSAttributedString(b.Value, new CoreText.CTStringAttributes() { ForegroundColor = color.CGColor, Font = ctFont });
                attributedString.Append(str);
                var strLength = str.Length;

                if (b.Tapped != null)
                    links.Add(new NewsCellView.Link { Range = new NSRange(lengthCounter, strLength), Callback = b.Tapped, Id = i++ });

                lengthCounter += strLength;
            }

            return new Tuple<NSMutableAttributedString, List<NewsCellView.Link>>(attributedString, links);
        }

        private static nfloat CharacterHeight 
        {
            get { return "A".MonoStringHeight(NewsCellView.BodyFont, 1000); }
        }

        public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            var s = 6f + NewsCellView.TimeFont.LineHeight + 5f + (NewsCellView.HeaderFont.LineHeight * 2) + 4f + 7f;

            if (_attributedBody.Length == 0)
                return s;

            var rec = _attributedBody.GetBoundingRect(new CGSize(tableView.Bounds.Width - 56, 10000), NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading, null);
            var height = rec.Height;

            if (_bodyBlocks == 1 && height > (CharacterHeight * 4))
                height = CharacterHeight * 4;

            var descCalc = s + height;
            var ret = ((int)Math.Ceiling(descCalc)) + 1f + 8f;
            return ret;
        }

        protected override NSString CellKey {
            get {
                return new NSString("NewsCellView");
            }
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(CellKey) as NewsCellView ?? NewsCellView.Create();
            return cell;
        }

        public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
        {
            base.Selected(dvc, tableView, path);
            if (_tapped != null)
                _tapped();
            tableView.DeselectRow (path, true);
        }

        void IColorizeBackground.WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            var c = cell as NewsCellView;
            if (c == null)
                return;

            UIImage image = null;
            if (_imageUri != null)
                image = ImageLoader.DefaultRequestImage(_imageUri, this);

            c.Set(image, _time, _actionImage, _attributedHeader, _attributedBody, _headerLinks, _bodyLinks, WebLinkClicked);
        }

        #region IImageUpdated implementation

        public void UpdatedImage(Uri uri)
        {
            var img = ImageLoader.DefaultRequestImage(uri, this);
            if (img == null)
                return;

            if (uri == null)
                return;
            var root = GetImmediateRootElement ();
            if (root == null || root.TableView == null)
                return;
            root.TableView.ReloadRows (new [] { IndexPath }, UITableViewRowAnimation.None);
        }

        #endregion
    }
}