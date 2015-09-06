// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;

namespace CodeHub.iOS.Views.Accounts
{
	[Register ("NewAccountView")]
	partial class NewAccountView
	{
		[Outlet]
		UIKit.UIButton EnterpriseButton { get; set; }

		[Outlet]
		UIKit.UIButton InternetButton { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (EnterpriseButton != null) {
				EnterpriseButton.Dispose ();
				EnterpriseButton = null;
			}

			if (InternetButton != null) {
				InternetButton.Dispose ();
				InternetButton = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}
		}
	}
}
