using System;
using UIKit;
using Foundation;
using SDWebImage;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CodeHub.iOS.DialogElements
{
    public class ButtonElement : StringElement
    {
        public ButtonElement (string caption, string value, UIImage image = null) 
            : base (caption, value, UITableViewCellStyle.Value1) 
        {
            Image = image;
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            SelectionStyle = UITableViewCellSelectionStyle.Blue;
        }

        public ButtonElement (string caption, UIImage image = null) 
            : this (caption, null, image)
        {
        }
    }

    public class StringElement : Element 
    {
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
        private Uri _imageUri;
        private string _value;
        private UITableViewCellAccessory _accessory = UITableViewCellAccessory.DisclosureIndicator;
        private readonly Subject<object> _tapped = new Subject<object>();
        private UITableViewCellSelectionStyle _selectionStyle = UITableViewCellSelectionStyle.Default;

        public IObservable<object> Clicked
        {
            get { return _tapped.AsObservable(); }
        }

        public UITableViewCellSelectionStyle SelectionStyle
        {
            get { return _selectionStyle; }
            set
            {
                _selectionStyle = value;
                var cell = GetActiveCell();
                if (cell != null)
                    cell.SelectionStyle = value;
            }
        }

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

        public UITableViewCellAccessory Accessory
        {
            get { return _accessory; }
            set
            {
                _accessory = value;
                var cell = GetActiveCell();
                if (cell != null)
                    cell.Accessory = value;
            }
        }

        private int _lines;
        public int Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                var cell = GetActiveCell();
                if (cell != null)
                    cell.TextLabel.Lines = value;
            }
        }

        private UILineBreakMode _lineBreakMode;
        public UILineBreakMode LineBreakMode
        {
            get { return _lineBreakMode; }
            set
            {
                _lineBreakMode = value;
                var cell = GetActiveCell();
                if (cell != null)
                    cell.TextLabel.LineBreakMode = value;
            }
        }

        public UIColor ImageTintColor { get; set; }

        public StringElement()
        {
            Font = DefaultTitleFont.WithSize(DefaultTitleFont.PointSize);
            SubtitleFont = DefaultDetailFont.WithSize(DefaultDetailFont.PointSize);
            TextColor = DefaultTitleColor;
        }

        public StringElement (string caption)
            : this()
        {
            Caption = caption;
        }

        public StringElement (string caption, UIImage image) 
            : this (caption) 
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

        public StringElement (string caption, UITableViewCellStyle style)
            : this (caption, null, style) 
        { 
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
            cell.SelectionStyle = SelectionStyle;
            cell.TextLabel.Text = Caption;
            cell.TextLabel.TextColor = TextColor;
            cell.ImageView.Image = Image;
            cell.Accessory = Accessory;

            if (ImageUri != null)
                cell.ImageView.SetImage(new NSUrl(ImageUri.AbsoluteUri), Image);

            if (cell.DetailTextLabel != null)
            {
                cell.DetailTextLabel.Text = Value ?? "";
                cell.DetailTextLabel.TextColor = DefaultDetailColor;
            }
            return cell;
        }

        public override void Selected (UITableView tableView, NSIndexPath indexPath)
        {
            base.Selected(tableView, indexPath);
            _tapped.OnNext(this);
        }

        public override bool Matches (string text)
        {
            return (Value != null && Value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) != -1) || base.Matches (text);
        }
    }
}

