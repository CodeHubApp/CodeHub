using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace CodeFramework.iOS.Cells
{
    public partial class RepositoryCellView : MvxTableViewCell
    {
        public static bool RoundImages = true;

        public static UIFont CaptionFont
        {
            get { return UIFont.BoldSystemFontOfSize(15f * Theme.CurrentTheme.FontSizeRatio); }
        }

        public static UIFont DescriptionFont
        {
            get { return UIFont.SystemFontOfSize(13f * Theme.CurrentTheme.FontSizeRatio); }
        }


        public static RepositoryCellView Create()
        {
            var cell = new RepositoryCellView();
            var views = NSBundle.MainBundle.LoadNib("RepositoryCellView", cell, null);
            cell = Runtime.GetNSObject( views.ValueAt(0) ) as RepositoryCellView;

            if (cell != null)
            {
                cell.Caption.TextColor = Theme.CurrentTheme.MainTitleColor;
                cell.Caption.Font = CaptionFont;

                cell.Description.TextColor = Theme.CurrentTheme.MainTextColor;
                cell.Description.Font = DescriptionFont;

                cell.Image1.Image = Theme.CurrentTheme.RepositoryCellFollowers;
                cell.Image3.Image = Theme.CurrentTheme.RepositoryCellForks;
                cell.UserImage.Image = Theme.CurrentTheme.RepositoryCellUser;

                if (RoundImages)
                {
                    cell.BigImage.Layer.MasksToBounds = true;
                    cell.BigImage.Layer.CornerRadius = cell.BigImage.Bounds.Height / 2f;
                }
            }

            //Create the icons
            return cell;
        }


        public RepositoryCellView()
        {
        }

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
        }

        public void Bind(string name, string name2, string name3, string description, string repoOwner, UIImage logoImage)
        {
            Caption.Text = name;
            Label1.Text = name2;
            Label3.Text = name3;
            BigImage.Image = logoImage;
            Description.Hidden = description == null;
            Description.Text = description ?? string.Empty;

            var frame = Description.Frame;
            frame.Y = 29f;
            frame.Height = this.Bounds.Height - frame.Y - 16f - 12f;
            Description.Frame = frame;

            RepoName.Hidden = repoOwner == null;
            UserImage.Hidden = RepoName.Hidden;
            RepoName.Text = repoOwner ?? string.Empty;
        }
    }
}

