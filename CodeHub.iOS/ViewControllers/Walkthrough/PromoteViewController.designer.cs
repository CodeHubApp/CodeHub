// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    [Register ("PromoteViewController")]
    partial class PromoteViewController
    {
        [Outlet]
        UIKit.UIButton StarButton { get; set; }

        [Outlet]
        UIKit.UIButton WatchButton { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (StarButton != null) {
                StarButton.Dispose ();
                StarButton = null;
            }

            if (WatchButton != null) {
                WatchButton.Dispose ();
                WatchButton = null;
            }
        }
    }
}
