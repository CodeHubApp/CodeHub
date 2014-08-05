using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.iOS;

namespace CodeFramework.iOS.Cells
{
    public partial class RepositoryCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("RepositoryCellView", NSBundle.MainBundle);
        public static NSString Key = new NSString("RepositoryCellView");
        public static bool RoundImages = true;

        public override string ReuseIdentifier { get { return Key; } }

        public static RepositoryCellView Create()
        {
            var cell = (RepositoryCellView)Nib.Instantiate(null, null)[0];

            cell.CaptionLabel.TextColor = Theme.CurrentTheme.MainTitleColor;
            cell.ContentLabel.TextColor = Theme.CurrentTheme.MainTextColor;

            cell.FollowersImageVIew.Image = Theme.CurrentTheme.RepositoryCellFollowers;
            cell.ForksImageView.Image = Theme.CurrentTheme.RepositoryCellForks;
            cell.UserImageView.Image = Theme.CurrentTheme.RepositoryCellUser;

            cell.OwnerImageView.Layer.MasksToBounds = true;
            cell.OwnerImageView.Layer.CornerRadius = cell.OwnerImageView.Bounds.Height / 2f;

            //Create the icons
            return cell;
        }

        public UIImage RepositoryImage
        {
            get { return OwnerImageView.Image; }
            set { OwnerImageView.Image = value; }
        }

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
        }

        public void Bind(string name, string name2, string name3, string description, string repoOwner, UIImage logoImage)
        {
            CaptionLabel.Text = name;
            FollowersLabel.Text = name2;
            ForksLabel.Text = name3;
            OwnerImageView.Image = logoImage;
            ContentLabel.Hidden = description == null;
            ContentLabel.Text = description ?? string.Empty;
            UserLabel.Hidden = repoOwner == null;
            UserImageView.Hidden = UserLabel.Hidden;
            UserLabel.Text = repoOwner ?? string.Empty;
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

