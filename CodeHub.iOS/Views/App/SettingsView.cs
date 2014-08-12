using CodeHub.Core.Services;
using MonoTouch.Dialog;
using CodeHub.Core.ViewModels.App;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

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

            var largeFonts = new BooleanElement("Large Fonts", vm.LargeFonts, x =>
            {
                vm.LargeFonts = x.Value;
                Theme.Setup();
                CreateTable();
            });

            accountSection.Add(new BooleanElement("Push Notifications", vm.PushNotificationsEnabled, e => vm.PushNotificationsEnabled = e.Value));

			var totalCacheSizeMB = vm.CacheSize.ToString("0.##");
			var deleteCache = new StyledStringElement("Delete Cache", string.Format("{0} MB", totalCacheSizeMB), MonoTouch.UIKit.UITableViewCellStyle.Value1);
			deleteCache.Tapped += () =>
			{ 
				vm.DeleteAllCacheCommand.Execute(null);
				deleteCache.Value = string.Format("{0} MB", 0);
				ReloadData();
			};

            var usage = new BooleanElement("Send Anonymous Usage", vm.AnalyticsEnabled, e => vm.AnalyticsEnabled = e.Value);

			//Assign the root
            Root.Add(accountSection);
            Root.Add(new Section("Appearance") { showOrganizationsInEvents, showOrganizations, repoDescriptions, startupView, largeFonts });
            Root.Add(new Section ("Internal") { deleteCache, usage });
		}
    }
}


