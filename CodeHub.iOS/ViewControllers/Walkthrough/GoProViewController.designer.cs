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
	[Register ("GoProViewController")]
	partial class GoProViewController
	{
		[Outlet]
		UIKit.UIButton TellMeMoreButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TellMeMoreButton != null) {
				TellMeMoreButton.Dispose ();
				TellMeMoreButton = null;
			}
		}
	}
}
