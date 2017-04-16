// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CodeHub.iOS.TableViewCells
{
    [Register("RepositoryCellView")]
    partial class RepositoryCellView
    {
        [Outlet]
        UIKit.UILabel CaptionLabel { get; set; }

        [Outlet]
        UIKit.NSLayoutConstraint ContentConstraint { get; set; }

        [Outlet]
        UIKit.UILabel ContentLabel { get; set; }

        [Outlet]
        UIKit.UIImageView FollowersImageVIew { get; set; }

        [Outlet]
        UIKit.UILabel FollowersLabel { get; set; }

        [Outlet]
        UIKit.UIImageView ForksImageView { get; set; }

        [Outlet]
        UIKit.UILabel ForksLabel { get; set; }

        [Outlet]
        UIKit.UIImageView OwnerImageView { get; set; }

        [Outlet]
        UIKit.UIImageView UserImageView { get; set; }

        [Outlet]
        UIKit.UILabel UserLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (CaptionLabel != null)
            {
                CaptionLabel.Dispose();
                CaptionLabel = null;
            }

            if (ContentLabel != null)
            {
                ContentLabel.Dispose();
                ContentLabel = null;
            }

            if (FollowersImageVIew != null)
            {
                FollowersImageVIew.Dispose();
                FollowersImageVIew = null;
            }

            if (FollowersLabel != null)
            {
                FollowersLabel.Dispose();
                FollowersLabel = null;
            }

            if (ForksImageView != null)
            {
                ForksImageView.Dispose();
                ForksImageView = null;
            }

            if (ForksLabel != null)
            {
                ForksLabel.Dispose();
                ForksLabel = null;
            }

            if (OwnerImageView != null)
            {
                OwnerImageView.Dispose();
                OwnerImageView = null;
            }

            if (UserImageView != null)
            {
                UserImageView.Dispose();
                UserImageView = null;
            }

            if (UserLabel != null)
            {
                UserLabel.Dispose();
                UserLabel = null;
            }

            if (ContentConstraint != null)
            {
                ContentConstraint.Dispose();
                ContentConstraint = null;
            }
        }
    }
}
