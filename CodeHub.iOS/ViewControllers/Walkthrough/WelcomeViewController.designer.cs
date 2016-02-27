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
    [Register ("WelcomeViewController")]
    partial class WelcomeViewController
    {
        [Outlet]
        UIKit.UIButton GoButton { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (GoButton != null) {
                GoButton.Dispose ();
                GoButton = null;
            }
        }
    }
}
