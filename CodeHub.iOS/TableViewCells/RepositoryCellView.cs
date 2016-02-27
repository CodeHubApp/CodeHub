using System;
using Foundation;
using ObjCRuntime;
using SDWebImage;
using CodeHub.iOS;
using MvvmCross.Binding.iOS.Views;
using CodeHub.Core.Utilities;
using UIKit;

namespace CodeHub.iOS.TableViewCells
{
    public partial class RepositoryCellView : MvxTableViewCell
    {
        public static NSString Key = new NSString("RepositoryCellView");

        public static RepositoryCellView Create()
        {
            var cell = new RepositoryCellView();
            var views = NSBundle.MainBundle.LoadNib("RepositoryCellView", cell, null);
            cell = Runtime.GetNSObject( views.ValueAt(0) ) as RepositoryCellView;

            if (cell != null)
            {
                cell.SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);

                cell.Caption.TextColor = Theme.CurrentTheme.MainTitleColor;
                cell.Description.TextColor = Theme.CurrentTheme.MainTextColor;

                cell.Image1.Image =  Octicon.Star.ToImage(12);
                cell.Image3.Image = Octicon.RepoForked.ToImage(12);
                cell.UserImage.Image = Octicon.Person.ToImage(12);

                cell.BigImage.Layer.MasksToBounds = true;
                cell.BigImage.Layer.CornerRadius = cell.BigImage.Bounds.Height / 2f;
            }

            //Create the icons
            return cell;
        }

        public override NSString ReuseIdentifier
        {
            get
            {
                return Key;
            }
        }

        public RepositoryCellView()
        {
        }

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
        }

        public void Bind(string name, string name2, string name3, string description, string repoOwner, GitHubAvatar imageUrl)
        {
            Caption.Text = name;
            Label1.Text = name2;
            Label3.Text = name3;
            Description.Hidden = description == null;
            Description.Text = description ?? string.Empty;

            RepoName.Hidden = repoOwner == null;
            UserImage.Hidden = RepoName.Hidden;
            RepoName.Text = repoOwner ?? string.Empty;

            BigImage.Image = Images.Avatar;

            try
            {
                var uri = imageUrl.ToUri(64)?.AbsoluteUri;
                if (uri != null)
                    BigImage.SetImage(new NSUrl(uri), Images.Avatar);
            }
            catch
            {
            }
        }
    }
}

