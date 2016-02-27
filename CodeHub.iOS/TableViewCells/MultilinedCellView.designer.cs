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
    [Register ("MultilinedCellView")]
    partial class MultilinedCellView
    {
        [Outlet]
        UIKit.UILabel CaptionLabel { get; set; }

        [Outlet]
        UIKit.UILabel DetailsLabel { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (CaptionLabel != null) {
                CaptionLabel.Dispose ();
                CaptionLabel = null;
            }

            if (DetailsLabel != null) {
                DetailsLabel.Dispose ();
                DetailsLabel = null;
            }
        }
    }
}
