using System;
using Foundation;
using UIKit;

namespace CodeHub.iOS.TableViewCells
{
    public partial class MultilinedCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("MultilinedCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("MultilinedCellView");

        public string Caption
        {
            get { return CaptionLabel.Text; }
            set { CaptionLabel.Text = value; }
        }

        public string Details
        {
            get { return DetailsLabel.Text; }
            set { DetailsLabel.Text = value; }
        }

        public MultilinedCellView(IntPtr handle)
            : base(handle)
        {
            SeparatorInset = UIEdgeInsets.Zero;
            PreservesSuperviewLayoutMargins = false;
            LayoutMargins = UIEdgeInsets.Zero;
        }

        public static MultilinedCellView Create()
        {
            var cell = (MultilinedCellView)Nib.Instantiate(null, null)[0];
            cell.CaptionLabel.TextColor = Theme.CurrentTheme.MainTextColor;
            cell.DetailsLabel.TextColor = Theme.CurrentTheme.MainTextColor;
            return cell;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();

            CaptionLabel.PreferredMaxLayoutWidth = CaptionLabel.Frame.Width;
            DetailsLabel.PreferredMaxLayoutWidth = DetailsLabel.Frame.Width;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            CaptionLabel.TextColor = DetailsLabel.TextColor = UIColor.FromRGB(41, 41, 41);
        }
    }
}

