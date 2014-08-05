
using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Cells
{
    public partial class GistCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("GistCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("GistCellView");

        public override string ReuseIdentifier { get { return Key; } }

        public GistCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static GistCellView Create()
        {
            var cell = (GistCellView)Nib.Instantiate(null, null)[0];
            cell.MainImageView.Layer.MasksToBounds = true;
            cell.MainImageView.Layer.CornerRadius = cell.MainImageView.Frame.Height / 2f;
            cell.SeparatorInset = new UIEdgeInsets(0, cell.TitleLabel.Frame.Left, 0, 0);
            cell.TitleLabel.TextColor = UIColor.FromRGB(0, 64, 128);
            cell.TimeLabel.TextColor = UIColor.Gray;
            cell.ContentLabel.TextColor = UIColor.FromRGB(41, 41, 41);
            return cell;
        }

        public UIImage Image 
        {
            get { return MainImageView.Image; }
            set { MainImageView.Image = value; }
        }

        public string Title
        {
            get { return TitleLabel.Text; }
            set { TitleLabel.Text = value; }
        }

        public string Content
        {
            get { return ContentLabel.Text; }
            set { ContentLabel.Text = value; }
        }

        public int Lines
        {
            get { return ContentLabel.Lines; }
            set { ContentLabel.Lines = value; }
        }

        public string Time
        {
            get { return TimeLabel.Text; }
            set { TimeLabel.Text = value; }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();

            ContentLabel.PreferredMaxLayoutWidth = ContentLabel.Frame.Width;
        }
    }
}

