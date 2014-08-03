// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace CodeFramework.iOS.Cells
{
	[Register ("RepositoryCellView")]
	partial class RepositoryCellView
	{
		[Outlet]
		MonoTouch.UIKit.UIImageView BigImage { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel Caption { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel Description { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView Image1 { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView Image3 { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel Label1 { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel Label3 { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel RepoName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView UserImage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BigImage != null) {
				BigImage.Dispose ();
				BigImage = null;
			}

			if (Caption != null) {
				Caption.Dispose ();
				Caption = null;
			}

			if (Description != null) {
				Description.Dispose ();
				Description = null;
			}

			if (Image1 != null) {
				Image1.Dispose ();
				Image1 = null;
			}

			if (Image3 != null) {
				Image3.Dispose ();
				Image3 = null;
			}

			if (Label1 != null) {
				Label1.Dispose ();
				Label1 = null;
			}

			if (Label3 != null) {
				Label3.Dispose ();
				Label3 = null;
			}

			if (RepoName != null) {
				RepoName.Dispose ();
				RepoName = null;
			}

			if (UserImage != null) {
				UserImage.Dispose ();
				UserImage = null;
			}
		}
	}
}
