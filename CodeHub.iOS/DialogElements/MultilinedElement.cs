using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace CodeHub.iOS.DialogElements
{
    public class MultilinedElement : CustomElement
    {
        private const float PaddingY = 10f;
        private const float PaddingX = 15f;

        private string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                var root = GetRootElement();
                if (root != null)
                {
                    root.Reload(this, UITableViewRowAnimation.None);
                }
            }
        }


        public UIFont CaptionFont { get; set; }
        public UIFont ValueFont { get; set; }
        public UIColor CaptionColor { get; set; }
        public UIColor ValueColor { get; set; }

        public UITableViewCellAccessory Accessory;

        public MultilinedElement(string caption, string value)
            : this(caption)
        {
            Value = value;
        }

        public MultilinedElement(string caption)
            : base(UITableViewCellStyle.Default, "multilinedelement")
        {
            Caption = caption;
            BackgroundColor = UIColor.White;
            CaptionFont = UIFont.SystemFontOfSize(15f);
            ValueFont = UIFont.SystemFontOfSize(13f);
            CaptionColor = ValueColor = UIColor.FromRGB(41, 41, 41);
            Accessory = UITableViewCellAccessory.None;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = base.GetCell(tv);
            cell.SeparatorInset = UIEdgeInsets.Zero;
            cell.Accessory = Accessory;
            return cell;
        }

        public override void Draw(CGRect bounds, CoreGraphics.CGContext context, UIView view)
        {
            view.BackgroundColor = UIColor.White;
            CaptionColor.SetColor();
            var width = bounds.Width - PaddingX * 2;
            var textHeight = Caption.MonoStringHeight(CaptionFont, width);
            new NSString(Caption).DrawString(new CGRect(PaddingX, PaddingY, width, bounds.Height - PaddingY * 2), CaptionFont, UILineBreakMode.WordWrap);

            if (Value != null)
            {
                ValueColor.SetColor();
                var valueOrigin = new CGPoint(PaddingX, PaddingY + textHeight + 8f);
                var valueSize = new CGSize(width, bounds.Height - valueOrigin.Y);
                new NSString(Value).DrawString(new CGRect(valueOrigin, valueSize), ValueFont, UILineBreakMode.WordWrap);
            }
        }

        public override float Height(UITableView tableView, CGRect bounds)
        {
            var cell = GetCell(tableView);
            cell.Bounds = new CGRect(0, 0, bounds.Width, 44f);
            cell.SetNeedsLayout();
            cell.LayoutIfNeeded();

            var contentWidth = cell.ContentView.Bounds.Width;
            var width = contentWidth - PaddingX * 2;
            var textHeight = Caption.MonoStringHeight(CaptionFont, width);

            if (Value != null)
            {
                textHeight += 6f;
                textHeight += Value.MonoStringHeight(ValueFont, width);
            }

            var height = (int)Math.Ceiling(textHeight + PaddingY * 2) + 1;
            return height;
        }
    }
}

