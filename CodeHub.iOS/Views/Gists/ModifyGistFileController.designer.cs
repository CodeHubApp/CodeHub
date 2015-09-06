// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace CodeHub.ViewControllers
{
	[Register ("ModifyGistFileController")]
	partial class ModifyGistFileController
	{
		[Outlet]
		UIKit.UITextField Name { get; set; }

		[Outlet]
		UIKit.UIView NameView { get; set; }

		[Outlet]
		UIKit.UIScrollView Scroll { get; set; }

		[Outlet]
		UIKit.UITextView Text { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Name != null) {
				Name.Dispose ();
				Name = null;
			}

			if (Scroll != null) {
				Scroll.Dispose ();
				Scroll = null;
			}

			if (Text != null) {
				Text.Dispose ();
				Text = null;
			}

			if (NameView != null) {
				NameView.Dispose ();
				NameView = null;
			}
		}
	}
}
