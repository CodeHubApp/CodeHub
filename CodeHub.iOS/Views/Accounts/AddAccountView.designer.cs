// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.Accounts
{
	[Register ("AddAccountView")]
	partial class AddAccountView
	{
		[Outlet]
		MonoTouch.UIKit.UITextField Domain { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView Logo { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField Password { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField User { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}

			if (Logo != null) {
				Logo.Dispose ();
				Logo = null;
			}

			if (Password != null) {
				Password.Dispose ();
				Password = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}

			if (User != null) {
				User.Dispose ();
				User = null;
			}

			if (Domain != null) {
				Domain.Dispose ();
				Domain = null;
			}
		}
	}
}
