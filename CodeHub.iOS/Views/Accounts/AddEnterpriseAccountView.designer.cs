// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CodeHub.iOS.Views.Accounts
{
	[Register ("AddEnterpriseAccountView")]
	partial class AddEnterpriseAccountView
	{
		[Outlet]
		UIKit.UITextField Domain { get; set; }

		[Outlet]
		UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		UIKit.UIImageView LogoImageView { get; set; }

		[Outlet]
		UIKit.UITextField Password { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		UIKit.UITextField Username { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LogoImageView != null) {
				LogoImageView.Dispose ();
				LogoImageView = null;
			}

			if (Domain != null) {
				Domain.Dispose ();
				Domain = null;
			}

			if (Username != null) {
				Username.Dispose ();
				Username = null;
			}

			if (Password != null) {
				Password.Dispose ();
				Password = null;
			}

			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}
		}
	}
}
