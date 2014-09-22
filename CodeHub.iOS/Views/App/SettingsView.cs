using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using ReactiveUI;

namespace CodeHub.iOS.Views.App
{
	public class SettingsView : ViewModelDialogViewController<SettingsViewModel>
	{
        public SettingsView()
        {
            Title = "Settings";
        }

        public override void ViewWillAppear(bool animated)
        {
//			vm.Bind(x => x.PushNotificationsEnabled, CreateTable);
//            vm.Bind(x => x.IsSaving, x =>
//            {
//                    if (x) _hud.Show("Saving...");
//                    else _hud.Hide();
//            });
			CreateTable();
            base.ViewWillAppear(animated);
        }

		private void CreateTable()
		{
			var application = IoC.Resolve<IApplicationService>();
			var vm = (SettingsViewModel)ViewModel;
			var currentAccount = application.Account;
            var accountSection = new Section("Account");

            accountSection.Add(new BooleanElement("Save Credentials", !currentAccount.DontRemember, e =>
            { 
//                currentAccount.DontRemember = !e.Value;
//                application.Accounts.Update(currentAccount);
            }));

            var showOrganizationsInEvents = new BooleanElement("Show Organizations in Events", currentAccount.ShowOrganizationsInEvents, e =>
			{ 
//				currentAccount.ShowOrganizationsInEvents = e.Value;
//				application.Accounts.Update(currentAccount);
			});

            var showOrganizations = new BooleanElement("List Organizations in Menu", currentAccount.ExpandOrganizations, e =>
			{ 
//				currentAccount.ExpandOrganizations = e.Value;
//				application.Accounts.Update(currentAccount);
			});

            var repoDescriptions = new BooleanElement("Show Repo Descriptions", currentAccount.ShowRepositoryDescriptionInList, e =>
			{ 
//				currentAccount.ShowRepositoryDescriptionInList = e.Value;
//				application.Accounts.Update(currentAccount);
			});

			var startupView = new StyledStringElement("Startup View", vm.DefaultStartupViewName, MonoTouch.UIKit.UITableViewCellStyle.Value1)
			{ 
				Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator,
			};
			startupView.Tapped += () => vm.GoToDefaultStartupViewCommand.Execute(null);

            //var sidebarOrder = new StyledStringElement("Sidebar Order", () => vm.GoToSidebarOrderCommand.Execute(null));

            accountSection.Add(new BooleanElement("Push Notifications", vm.PushNotificationsEnabled, e => vm.PushNotificationsEnabled = e.Value));

			//Assign the root
            Root.Add(accountSection);
            Root.Add(new Section("Appearance") { showOrganizationsInEvents, showOrganizations, repoDescriptions, startupView });

            Root.Add(new Section("About", "Thank you for downloading. Enjoy!")
            {
                new StyledStringElement("Follow On Twitter", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp"))),
                new StyledStringElement("Rate This App", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8"))),
                new StyledStringElement("Source Code", () => ViewModel.GoToSourceCodeCommand.ExecuteIfCan()),
                new StyledStringElement("App Version", ViewModel.Version)
            });
		}
    }
}


