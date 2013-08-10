// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace CodeHub.GitHub.Controllers.Accounts
{
    [Register ("LoginViewController")]
    partial class LoginViewController
	{
		[Outlet]
		protected virtual MonoTouch.UIKit.UITextField User { get; set; }

		[Outlet]
		protected virtual MonoTouch.UIKit.UITextField Password { get; set; }

		[Outlet]
		protected virtual MonoTouch.UIKit.UIImageView Logo { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (User != null) {
				User.Dispose ();
				User = null;
			}

			if (Password != null) {
				Password.Dispose ();
				Password = null;
			}

			if (Logo != null) {
				Logo.Dispose ();
				Logo = null;
			}
		}
	}
}
