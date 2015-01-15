using System;
using Foundation;
using UIKit;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Services
{
	public class ShareService : IShareService
    {
		public void ShareUrl(Uri uri)
		{
			var item = new NSUrl(uri.AbsoluteUri);
			var activityItems = new NSObject[] { item };
			UIActivity[] applicationActivities = null;
			var activityController = new UIActivityViewController (activityItems, applicationActivities);

            var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
            appDelegate.Window.RootViewController.PresentViewController(activityController, true, null);
		}
    }
}

