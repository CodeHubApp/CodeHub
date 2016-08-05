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
	[Register ("NewsCellView")]
	partial class NewsCellView
	{
		[Outlet]
		UIKit.UIImageView ActionImage { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint AdjustableConstraint { get; set; }

		[Outlet]
		Xamarin.TTTAttributedLabel.TTTAttributedLabel Body { get; set; }

		[Outlet]
		Xamarin.TTTAttributedLabel.TTTAttributedLabel Header { get; set; }

		[Outlet]
		UIKit.UIImageView Image { get; set; }

		[Outlet]
		UIKit.UILabel Time { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ActionImage != null) {
				ActionImage.Dispose ();
				ActionImage = null;
			}

			if (Body != null) {
				Body.Dispose ();
				Body = null;
			}

			if (Header != null) {
				Header.Dispose ();
				Header = null;
			}

			if (Image != null) {
				Image.Dispose ();
				Image = null;
			}

			if (Time != null) {
				Time.Dispose ();
				Time = null;
			}

			if (AdjustableConstraint != null) {
				AdjustableConstraint.Dispose ();
				AdjustableConstraint = null;
			}
		}
	}
}
