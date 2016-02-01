using System;
using UIKit;
using CoreGraphics;
using CoreGraphics;
using MonoTouch.Dialog.Utilities;
using Foundation;

namespace MonoTouch.Dialog
{
    public class NameTimeStringElement : CustomElement, IImageUpdated
    {
        public static UIFont DateFont = UIFont.SystemFontOfSize(12);
        public static UIFont UserFont = UIFont.BoldSystemFontOfSize(15);
        public static UIFont DescFont = UIFont.SystemFontOfSize(12);

        public static UIColor TextColor = UIColor.FromRGB(41, 41, 41);
        public static UIColor NameColor = UIColor.FromRGB(0, 64, 128);
        public static UIColor TimeColor = UIColor.Gray;

        public static nfloat LeftRightPadding = 6f;
        public static nfloat TopBottomPadding = 8f;

        public string Name { get; set; }
        public string Time { get; set; }
        public string String { get; set; }
        public Uri ImageUri { get; set; }

        public int Lines { get; set; }
        public UIImage Image { get; set; }

        private UIFont _dateFont, _userFont, _descFont;

        private bool IsImageAssigned
        {
            get { return Image != null || ImageUri != null; }
        }

        public NameTimeStringElement() 
            : base(UITableViewCellStyle.Default, "nametimestringelement")
        {
            Lines = 9999;
			BackgroundColor = UIColor.White;
            _dateFont = DateFont.WithSize(DateFont.PointSize * Element.FontSizeRatio);
            _userFont = UserFont.WithSize(UserFont.PointSize * Element.FontSizeRatio);
            _descFont = DescFont.WithSize(DescFont.PointSize * Element.FontSizeRatio);
        }

        public override void Draw(CGRect bounds, CGContext context, UIView view)
        {
            var leftMargin = LeftRightPadding;

            // Superview is the container, its superview the uitableviewcell
            var uiTableViewCell = view.Superview.Superview as UITableViewCell;
            bool highlighted = uiTableViewCell != null && uiTableViewCell.Highlighted & IsTappedAssigned;
            var timeColor = highlighted ? UIColor.White : TimeColor;
            var textColor = highlighted ? UIColor.White : TextColor;
            var nameColor = highlighted ? UIColor.White : NameColor;
//
//			UIColor.White.SetFill();
//			context.FillRect(bounds);
//
            if (Image != null)
            {
                var imageRect = new CGRect(LeftRightPadding, TopBottomPadding, 32f, 32f);
                UIColor.White.SetColor ();

                context.SaveState ();
                UIBezierPath.FromRoundedRect(imageRect, imageRect.Width / 2).AddClip();
                Image.Draw(imageRect);
                context.RestoreState();
                leftMargin += LeftRightPadding + imageRect.Width + 3f;
            }

            var contentWidth = bounds.Width - LeftRightPadding  - leftMargin;

            nameColor.SetColor();
            new NSString(Name).DrawString(
                new CGRect(leftMargin, TopBottomPadding, contentWidth, _userFont.LineHeight),
                _userFont, UILineBreakMode.TailTruncation
            );

            timeColor.SetColor();
            var daysWidth = Time.MonoStringLength(_dateFont);
            var timeRect = IsImageAssigned ? new CGRect(leftMargin, TopBottomPadding + _userFont.LineHeight, daysWidth, _dateFont.LineHeight) : 
                new CGRect(bounds.Width - LeftRightPadding - daysWidth,  TopBottomPadding + 1f, daysWidth, _dateFont.LineHeight);
            new NSString(Time).DrawString(timeRect, _dateFont, UILineBreakMode.TailTruncation);

            if (!string.IsNullOrEmpty(String))
            {
                UIColor.Black.SetColor();
                var top = TopBottomPadding + _userFont.LineHeight + 3f;
                if (IsImageAssigned)
                    top += _dateFont.LineHeight;

                textColor.SetColor();
                var stringRect = IsImageAssigned ? new CGRect(leftMargin, top, contentWidth, bounds.Height - TopBottomPadding - top) :
				                 new CGRect(LeftRightPadding, top, bounds.Width - LeftRightPadding*2, bounds.Height - TopBottomPadding - top);

                new NSString(String).DrawString(stringRect, _descFont, UILineBreakMode.TailTruncation);
            }
        }

        public override float Height(UITableView tableView, CGRect bounds)
        {
            var contentWidth = bounds.Width - LeftRightPadding * 2; //Account for the Accessory
            if (IsTappedAssigned)
                contentWidth -= 20f;
            if (IsImageAssigned)
                contentWidth -= (LeftRightPadding + 32f + 3f);

            var descHeight = this.String.MonoStringHeight(_descFont, contentWidth);
            if (descHeight > (_descFont.LineHeight) * Lines)
                descHeight = (_descFont.LineHeight) * Lines;

            var n = TopBottomPadding*2 + _userFont.LineHeight + 3f + descHeight;
            if (IsImageAssigned)
                n += _dateFont.LineHeight;
			var ret = (int)Math.Ceiling(n) + 1;
			return ret ;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            if (ImageUri != null)
            {
                var img = ImageLoader.DefaultRequestImage(ImageUri, this);
                if (img != null)
                    Image = img;
            }

			var cell = base.GetCell(tv);
			//TableView.SeparatorInset = new MonoTouch.UIKit.UIEdgeInsets(0, 47f, 0, 0);
			cell.SeparatorInset = UIEdgeInsets.Zero;
			return cell;
        }

        public override bool Matches(string text)
        {
            var ltext = text.ToLower();
            return base.Matches(text) || Name.ToLower().Contains(ltext) || String.ToLower().Contains(ltext);
        }

        void IImageUpdated.UpdatedImage (Uri uri)
        {
            var img = ImageLoader.DefaultRequestImage(uri, this);
            if (img == null)
            {
                Console.WriteLine("DefaultRequestImage returned a null image");
                return;
            }
            Image = img;

            if (uri == null)
                return;
            var root = GetImmediateRootElement ();
            if (root == null || root.TableView == null)
                return;
            root.TableView.ReloadRows (new NSIndexPath [] { IndexPath }, UITableViewRowAnimation.None);
        }
    }
}

