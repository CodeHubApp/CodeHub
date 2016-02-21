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
    [Register ("FeedbackViewController")]
    partial class FeedbackViewController
    {
        [Outlet]
        UIKit.UIButton GitHubButton { get; set; }

        [Outlet]
        UIKit.UIButton UserVoiceButton { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (GitHubButton != null) {
                GitHubButton.Dispose ();
                GitHubButton = null;
            }

            if (UserVoiceButton != null) {
                UserVoiceButton.Dispose ();
                UserVoiceButton = null;
            }
        }
    }
}
