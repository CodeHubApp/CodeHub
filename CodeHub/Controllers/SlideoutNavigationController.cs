using System;
using MonoTouch.UIKit;
using CodeHub.Data;

namespace CodeHub.Controllers
{
	public class SlideoutNavigationController : CodeFramework.Controllers.SlideoutNavigationController
	{
		private Account _previousUser;

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			//This shouldn't happen
			if (Application.Accounts.ActiveAccount == null)
				return;

			//Determine which menu to instantiate by the account type!
			MenuView = new MenuController();

            //The previous user was returning from the settings menu. Nothing has changed as far as current user goes
            if (Application.Accounts.ActiveAccount.Equals(_previousUser))
                return;
            _previousUser = Application.Accounts.ActiveAccount;


            //Select a view based on the account type
            SelectView(new CodeHub.GitHub.Controllers.Events.EventsController(Application.Accounts.ActiveAccount.Username));
		}
	}
}

