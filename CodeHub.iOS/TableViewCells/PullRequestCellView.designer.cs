// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CodeHub.iOS.Cells
{
    [Register ("PullRequestCellView")]
    partial class PullRequestCellView
    {
        [Outlet]
        UIKit.NSLayoutConstraint ContentConstraint { get; set; }

        [Outlet]
        UIKit.UIImageView MainImageView { get; set; }

        [Outlet]
        UIKit.UILabel TimeLabel { get; set; }

        [Outlet]
        UIKit.UILabel TitleLabel { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (ContentConstraint != null) {
                ContentConstraint.Dispose ();
                ContentConstraint = null;
            }

            if (MainImageView != null) {
                MainImageView.Dispose ();
                MainImageView = null;
            }

            if (TimeLabel != null) {
                TimeLabel.Dispose ();
                TimeLabel = null;
            }

            if (TitleLabel != null) {
                TitleLabel.Dispose ();
                TitleLabel = null;
            }
        }
    }
}
