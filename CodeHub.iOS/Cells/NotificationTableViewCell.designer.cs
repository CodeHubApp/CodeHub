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
	[Register ("NotificationTableViewCell")]
	partial class NotificationTableViewCell
	{
		[Outlet]
		UIKit.UILabel DescriptionLabel { get; set; }

		[Outlet]
		UIKit.UIImageView IconImageView { get; set; }

		[Outlet]
		UIKit.UILabel TimeLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (IconImageView != null) {
				IconImageView.Dispose ();
				IconImageView = null;
			}

			if (TimeLabel != null) {
				TimeLabel.Dispose ();
				TimeLabel = null;
			}

			if (DescriptionLabel != null) {
				DescriptionLabel.Dispose ();
				DescriptionLabel = null;
			}
		}
	}
}
