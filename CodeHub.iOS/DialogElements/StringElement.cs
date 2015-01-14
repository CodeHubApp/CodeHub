using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using SDWebImage;
using ReactiveUI;

namespace CodeHub.iOS.DialogElements
{
    public class StringElement : Element 
    {
        private static NSString Key = new NSString("StringElement");
        public UITextAlignment Alignment = UITextAlignment.Left;
        public string Value;
        public event Action Tapped;

        public StringElement (string caption)
            : this(caption, null)
        {
            this.Caption = caption;
        }

        public StringElement (string caption,  string value, IReactiveCommand tapped)
            : this(caption, value, () => tapped.ExecuteIfCan())
        {
        }

        public StringElement (string caption,  string value, Action tapped = null)
        {
            this.Caption = caption;
            this.Value = value;

            if (tapped != null)
                Tapped += tapped;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell (Key);
            if (cell == null)
            {
                cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Value1, Key);
                cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Blue : UITableViewCellSelectionStyle.None;
            }
            cell.Accessory = UITableViewCellAccessory.None;
            cell.TextLabel.Text = Caption;
            cell.TextLabel.TextAlignment = Alignment;
            if (cell.DetailTextLabel != null)
                cell.DetailTextLabel.Text = Value ?? "";
            return cell;
        }

        public override void Selected (UITableView tableView, NSIndexPath indexPath)
        {
            base.Selected(tableView, indexPath);
            if (Tapped != null)
                Tapped ();
        }

        public override bool Matches (string text)
        {
            return (Value != null && Value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) != -1) || base.Matches (text);
        }
    }


    /// <summary>
    ///   A version of the StringElement that can be styled with a number of formatting 
    ///   options and can render images or background images either from UIImage parameters 
    ///   or by downloading them from the net.
    /// </summary>
    public class StyledStringElement : StringElement 
    {
        public static UIFont  DefaultTitleFont = UIFont.SystemFontOfSize(16f);
        public static UIFont  DefaultDetailFont = UIFont.SystemFontOfSize(13f);
        public static UIColor DefaultTitleColor = UIColor.FromRGB(41, 41, 41);
        public static UIColor DefaultDetailColor = UIColor.FromRGB(100, 100, 100);
        public static UIColor BgColor = UIColor.White;

        static NSString [] skey = { new NSString (".1"), new NSString (".2"), new NSString (".3"), new NSString (".4") };

        public StyledStringElement (string caption) : base (caption) 
        {
            Init();
        }

        public StyledStringElement (string caption, Action tapped) : base (caption, null, tapped) 
        {
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            Init();
        }

        public StyledStringElement (string caption, Action tapped, UIImage image) : base (caption, null, tapped) 
        {
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            Init();
            Image = image;
        }


        public StyledStringElement (string caption, string value) : base (caption, value) 
        {
            style = UITableViewCellStyle.Value1;
            Init();
        }
        public StyledStringElement (string caption, string value, UITableViewCellStyle style) : base (caption, value) 
        { 
            this.style = style;
            Init();
        }

        protected UITableViewCellStyle style;
        public event NSAction AccessoryTapped;
        public UIFont Font;
        public UIFont SubtitleFont;
        public UIColor TextColor;
        public UILineBreakMode LineBreakMode = UILineBreakMode.WordWrap;
        public int Lines = 1;
        public UITableViewCellAccessory Accessory = UITableViewCellAccessory.None;

        private void Init()
        {
            Font = DefaultTitleFont.WithSize(DefaultTitleFont.PointSize);
            SubtitleFont = DefaultDetailFont.WithSize(DefaultDetailFont.PointSize);
            BackgroundColor = BgColor;
            TextColor = DefaultTitleColor;
            DetailColor = DefaultDetailColor;
            LineBreakMode = UILineBreakMode.TailTruncation;
        }

        // To keep the size down for a StyleStringElement, we put all the image information
        // on a separate structure, and create this on demand.
        ExtraInfo extraInfo;

        class ExtraInfo {
            public UIImage Image; // Maybe add BackgroundImage?
            public UIColor BackgroundColor, DetailColor;
            public Uri Uri;
        }

        ExtraInfo OnImageInfo ()
        {
            if (extraInfo == null)
                extraInfo = new ExtraInfo ();
            return extraInfo;
        }

        // Uses the specified image (use this or ImageUri)
        public UIImage Image {
            get {
                return extraInfo == null ? null : extraInfo.Image;
            }
            set {
                OnImageInfo ().Image = value;
                extraInfo.Uri = null;
            }
        }

        // Loads the image from the specified uri (use this or Image)
        public Uri ImageUri {
            get {
                return extraInfo == null ? null : extraInfo.Uri;
            }
            set {
                OnImageInfo ().Uri = value;
                extraInfo.Image = null;
            }
        }

        // Background color for the cell (alternative: BackgroundUri)
        public UIColor BackgroundColor {
            get {
                return extraInfo == null ? null : extraInfo.BackgroundColor;
            }
            set {
                OnImageInfo ().BackgroundColor = value;
            }
        }

        public UIColor DetailColor {
            get {
                return extraInfo == null ? null : extraInfo.DetailColor;
            }
            set {
                OnImageInfo ().DetailColor = value;
            }
        }

        protected virtual string GetKey (int style)
        {
            return skey [style];
        }

        protected virtual void OnCellCreated(UITableViewCell cell)
        {
        }

        protected virtual UITableViewCell CreateTableViewCell(UITableViewCellStyle style, string key)
        {
            return new UITableViewCell (style, key);
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var key = GetKey ((int) style);
            var cell = tv.DequeueReusableCell (key);
            if (cell == null){
                cell = CreateTableViewCell(style, key);
                OnCellCreated(cell);
                cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
            }
            PrepareCell (cell);

            ClearBackground(cell);

            if (extraInfo != null)
            {
                if (extraInfo.BackgroundColor != null)
                {
                    cell.BackgroundColor = extraInfo.BackgroundColor;
                    cell.TextLabel.BackgroundColor = UIColor.Clear;
                }
            }

            return cell;
        }

        protected void PrepareCell (UITableViewCell cell)
        {
            cell.Accessory = Accessory;
            var tl = cell.TextLabel;
            tl.Text = Caption;
            tl.TextAlignment = Alignment;
            tl.TextColor = TextColor ?? UIColor.Black;
            tl.Font = Font ?? UIFont.BoldSystemFontOfSize (17);
            tl.LineBreakMode = LineBreakMode;
            tl.Lines = Lines;   

            // The check is needed because the cell might have been recycled.
            if (cell.DetailTextLabel != null)
                cell.DetailTextLabel.Text = Value == null ? "" : Value;

            if (extraInfo == null){
                ClearBackground (cell);
            } else {
                var imgView = cell.ImageView;

                if (imgView != null) {
                    if (extraInfo.Uri != null)
                    {
                        imgView.SetImage(new NSUrl(extraInfo.Uri.AbsoluteUri));
                    }
                    else if (extraInfo.Image != null)
                    {
                        imgView.Image = extraInfo.Image;
                    }
                    else
                    {
                        imgView.Image = null;
                    }
                }

                if (cell.DetailTextLabel != null)
                    cell.DetailTextLabel.TextColor = extraInfo.DetailColor ?? UIColor.Gray;
            }

            if (cell.DetailTextLabel != null){
                cell.DetailTextLabel.Lines = Lines;
                cell.DetailTextLabel.LineBreakMode = LineBreakMode;
                cell.DetailTextLabel.Font = SubtitleFont ?? UIFont.SystemFontOfSize (14);
                cell.DetailTextLabel.TextColor = (extraInfo == null || extraInfo.DetailColor == null) ? UIColor.Gray : extraInfo.DetailColor;
            }
        }   

        void ClearBackground (UITableViewCell cell)
        {
            cell.BackgroundColor = UIColor.White;
            cell.TextLabel.BackgroundColor = UIColor.Clear;

            if (cell.DetailTextLabel != null)
                cell.DetailTextLabel.BackgroundColor = UIColor.Clear;
        }

        internal void AccessoryTap ()
        {
            NSAction tapped = AccessoryTapped;
            if (tapped != null)
                tapped ();
        }
    }



    public class StyledMultilineElement : StyledStringElement, IElementSizing 
    {
        public StyledMultilineElement (string caption) : base (caption) {}
        public StyledMultilineElement (string caption, string value) : base (caption, value) {}
        public StyledMultilineElement (string caption, Action tapped) : base (caption, tapped) {}
        public StyledMultilineElement (string caption, string value, UITableViewCellStyle style) : base (caption, value) 
        { 
            this.style = style;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = base.GetCell (tv);
            cell.SeparatorInset = UIEdgeInsets.Zero;
            var tl = cell.TextLabel;
            tl.LineBreakMode = UILineBreakMode.WordWrap;
            tl.Lines = 0;

            var sl = cell.DetailTextLabel;
            if (sl != null) {
                sl.LineBreakMode = UILineBreakMode.WordWrap;
                sl.Lines = 0;
            }

            cell.SeparatorInset = UIEdgeInsets.Zero;
            return cell;
        }

        public virtual float GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            const float margin = 30f;
            SizeF maxSize = new SizeF (tableView.Bounds.Width - margin, float.MaxValue);

            if (this.Accessory != UITableViewCellAccessory.None)
                maxSize.Width -= 20;

            string c = Caption;
            string v = Value;
            // ensure the (multi-line) Value will be rendered inside the cell when no Caption is present
            if (String.IsNullOrEmpty (c) && !String.IsNullOrEmpty (v))
                c = " ";

            var captionFont = Font ?? UIFont.BoldSystemFontOfSize (17);
            float height = tableView.StringSize (c, captionFont, maxSize, LineBreakMode).Height;

            if (!String.IsNullOrEmpty (v)) {
                var subtitleFont = SubtitleFont ?? UIFont.SystemFontOfSize (14);
                if (this.style == UITableViewCellStyle.Subtitle) {
                    height += tableView.StringSize (v, subtitleFont, maxSize, LineBreakMode).Height;
                } else {
                    float vheight = tableView.StringSize (v, subtitleFont, maxSize, LineBreakMode).Height;
                    if (vheight > height)
                        height = vheight;
                }
            }

            return height + 10;
        }
    }
}

