using System;
using MonoTouch.UIKit;
using CodeHub.Data;
using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
	public class SlideoutNavigationViewController : CodeFramework.Controllers.SlideoutNavigationController
	{
		private Account _previousUser;

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			//This shouldn't happen
			if (Application.Account == null)
				return;

			//Determine which menu to instantiate by the account type!
			MenuView = new MenuViewController();

            //The previous user was returning from the settings menu. Nothing has changed as far as current user goes
            if (Application.Account.Equals(_previousUser))
                return;
            _previousUser = Application.Account;


            //Select a view based on the account type
            SelectView(new NewsViewController());
		}
	}
}

