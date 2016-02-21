// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    [Register ("PrivateRepositoryViewController")]
    partial class PrivateRepositoryViewController
    {
        [Outlet]
        UIKit.UIButton Button { get; set; }

        [Outlet]
        UIKit.UILabel Description { get; set; }

        [Outlet]
        UIKit.UIImageView ImageView { get; set; }

        [Outlet]
        UIKit.UILabel Title { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (ImageView != null) {
                ImageView.Dispose ();
                ImageView = null;
            }

            if (Description != null) {
                Description.Dispose ();
                Description = null;
            }

            if (Button != null) {
                Button.Dispose ();
                Button = null;
            }

            if (Title != null) {
                Title.Dispose ();
                Title = null;
            }
        }
    }
}
