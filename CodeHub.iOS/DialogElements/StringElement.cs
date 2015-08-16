using System;
using UIKit;
using Foundation;
using SDWebImage;
using Humanizer;
using ReactiveUI;

namespace CodeHub.iOS.DialogElements
{
    public class StringElement : Element 
    {
        public static UIColor DefaultTintColor = null;
        public static UIFont  DefaultTitleFont = UIFont.PreferredBody;
        public static UIFont  DefaultDetailFont = UIFont.PreferredSubheadline;
        public static UIColor DefaultTitleColor = UIColor.FromRGB(41, 41, 41);
        public static UIColor DefaultDetailColor = UIColor.FromRGB(80, 80, 80);
        public static UIColor BgColor = UIColor.White;

        static NSString [] skey = { new NSString (".1"), new NSString (".2"), new NSString (".3"), new NSString (".4") };

        public UITableViewCellStyle Style;
        public UIFont Font;
        public UIFont SubtitleFont;
        public UIColor TextColor;
        private UIImage _image;
        public UIColor BackgroundColor, DetailColor;
        private Uri _imageUri;
        private string _value;
        private UITableViewCellAccessory _accessory = UITableViewCellAccessory.None;
        private Action _tapped;
        public Action AccessoryTapped;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                var cell = GetActiveCell();
                if (cell != null && cell.DetailTextLabel != null)
                    cell.DetailTextLabel.Text = value ?? string.Empty;
            }
        }

        public Action Tapped
        {
            get { return _tapped; }
            set
            {
                _tapped = value;
                Accessory = (value == null ? UITableViewCellAccessory.None : UITableViewCellAccessory.DisclosureIndicator);
            }
        }

        public UITableViewCellAccessory Accessory
        {
            get { return _accessory; }
            set
            {
                _accessory = value;
                var cell = GetActiveCell();
                if (cell != null) cell.Accessory = value;
            }
        }

        public UIColor ImageTintColor { get; set; }

        public StringElement()
        {
            Font = DefaultTitleFont.WithSize(DefaultTitleFont.PointSize);
            SubtitleFont = DefaultDetailFont.WithSize(DefaultDetailFont.PointSize);
            BackgroundColor = BgColor;
            TextColor = DefaultTitleColor;
            DetailColor = DefaultDetailColor;
        }

        public StringElement (UIImage image)
            : this()
        {
            Image = image;
        }

        public StringElement (string caption)
            : this()
        {
            Caption = caption;
        }

        public StringElement (string caption, Action tapped) 
            : this(caption)
        {
            Tapped = tapped;
        }

        public StringElement (string caption, Action tapped, UIImage image) 
            : this (caption, tapped) 
        {
            Image = image;
        }

        public StringElement (string caption, string value) 
            : this (caption) 
        {
            Style = UITableViewCellStyle.Value1;
            Value = value;
        }

        public StringElement (string caption, string value, UITableViewCellStyle style)
            : this (caption, value) 
        { 
            this.Style = style;
        }

        public UIImage Image {
            get { return _image; }
            set 
            {
                _image = value;
                var cell = GetActiveCell();
                if (cell != null)
                    cell.ImageView.Image = value;
            }
        }

        // Loads the image from the specified uri (use this or Image)
        public Uri ImageUri {
            get { return _imageUri; }
            set {
                _imageUri = value;
                var cell = GetActiveCell();
                if (cell != null)
                    cell.ImageView.SetImage(new NSUrl(value.AbsoluteUri));
            }
        }

        protected virtual string GetKey (int style)
        {
            return skey [style];
        }

        protected virtual UITableViewCell CreateTableViewCell(UITableViewCellStyle style, string key)
        {
            return new UITableViewCell (style, key);
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var key = GetKey ((int) Style);
            var cell = tv.DequeueReusableCell(key) ?? CreateTableViewCell(Style, key);
            return InitializeCell(cell);
        }

        protected virtual UITableViewCell InitializeCell(UITableViewCell cell)
        {
            cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Blue : UITableViewCellSelectionStyle.None;
            cell.TextLabel.Text = Caption;
            cell.ImageView.Image = Image;
            cell.ImageView.TintColor = ImageTintColor ?? DefaultTintColor;
            cell.Accessory = Accessory;

            if (ImageUri != null)
                cell.ImageView.SetImage(new NSUrl(ImageUri.AbsoluteUri), Image);

            if (cell.DetailTextLabel != null)
                cell.DetailTextLabel.Text = Value ?? "";
            return cell;
        }

        internal void AccessoryTap ()
        {
            var tapped = AccessoryTapped;
            if (tapped != null)
                tapped ();
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
}

