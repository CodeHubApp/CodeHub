// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;
using MonoTouch.TTTAttributedLabel;

namespace CodeHub.iOS.Cells
{
	[Register ("NewsCellView")]
	partial class NewsCellView
	{
		[Outlet]
		UIKit.UIImageView ActionImage { get; set; }

		[Outlet]
        TTTAttributedLabel Body { get; set; }

		[Outlet]
        TTTAttributedLabel Header { get; set; }

		[Outlet]
		UIKit.UIImageView Image { get; set; }

		[Outlet]
		UIKit.UILabel Time { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Image != null) {
				Image.Dispose ();
				Image = null;
			}

			if (Time != null) {
				Time.Dispose ();
				Time = null;
			}

			if (ActionImage != null) {
				ActionImage.Dispose ();
				ActionImage = null;
			}
		}
	}
}
