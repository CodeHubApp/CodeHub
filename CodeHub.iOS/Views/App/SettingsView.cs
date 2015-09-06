using Cirrious.CrossCore;
using CodeHub.Core.Services;
using MonoTouch.Dialog;
using CodeFramework.iOS.ViewControllers;
using CodeHub.Core.ViewModels.App;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views.App
{
	public class SettingsView : ViewModelDrivenDialogViewController
    {
        private IHud _hud;

        public SettingsView()
        {
            Title = "Settings";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _hud = new Hud(View);
        }

        public override void ViewWillAppear(bool animated)
        {
			var vm = (SettingsViewModel)ViewModel;
			vm.Bind(x => x.PushNotificationsEnabled, CreateTable);
            vm.Bind(x => x.IsSaving, x =>
            {
                    if (x) _hud.Show("Saving...");
                    else _hud.Hide();
            });
			CreateTable();
            base.ViewWillAppear(animated);
        }

		private void CreateTable()
		{
			var application = Mvx.Resolve<IApplicationService>();
			var vm = (SettingsViewModel)ViewModel;
			var currentAccount = application.Account;
            var accountSection = new Section("Account");

            accountSection.Add(new TrueFalseElement("Save Credentials".t(), !currentAccount.DontRemember, e =>
            { 
                currentAccount.DontRemember = !e.Value;
                application.Accounts.Update(currentAccount);
            }));

			var showOrganizationsInEvents = new TrueFalseElement("Show Organizations in Events".t(), currentAccount.ShowOrganizationsInEvents, e =>
			{ 
				currentAccount.ShowOrganizationsInEvents = e.Value;
				application.Accounts.Update(currentAccount);
			});

			var showOrganizations = new TrueFalseElement("List Organizations in Menu".t(), currentAccount.ExpandOrganizations, e =>
			{ 
				currentAccount.ExpandOrganizations = e.Value;
				application.Accounts.Update(currentAccount);
			});

			var repoDescriptions = new TrueFalseElement("Show Repo Descriptions".t(), currentAccount.ShowRepositoryDescriptionInList, e =>
			{ 
				currentAccount.ShowRepositoryDescriptionInList = e.Value;
				application.Accounts.Update(currentAccount);
			});

			var startupView = new StyledStringElement("Startup View", vm.DefaultStartupViewName, UIKit.UITableViewCellStyle.Value1)
			{ 
				Accessory = UIKit.UITableViewCellAccessory.DisclosureIndicator,
			};
			startupView.Tapped += () => vm.GoToDefaultStartupViewCommand.Execute(null);

            var sidebarOrder = new StyledStringElement("Sidebar Order", () => vm.GoToSidebarOrderCommand.Execute(null));

            var largeFonts = new TrueFalseElement("Large Fonts", vm.LargeFonts, x =>
            {
                vm.LargeFonts = x.Value;
                Theme.Setup();
                CreateTable();
            });

            accountSection.Add(new TrueFalseElement("Push Notifications".t(), vm.PushNotificationsEnabled, e => vm.PushNotificationsEnabled = e.Value));

			var totalCacheSizeMB = vm.CacheSize.ToString("0.##");
			var deleteCache = new StyledStringElement("Delete Cache".t(), string.Format("{0} MB", totalCacheSizeMB), UIKit.UITableViewCellStyle.Value1);
			deleteCache.Tapped += () =>
			{ 
				vm.DeleteAllCacheCommand.Execute(null);
				deleteCache.Value = string.Format("{0} MB", 0);
				ReloadData();
			};

			var usage = new TrueFalseElement("Send Anonymous Usage".t(), vm.AnalyticsEnabled, e => vm.AnalyticsEnabled = e.Value);

			//Assign the root
			var root = new RootElement(Title);
            root.Add(accountSection);
            root.Add(new Section("Appearance") { showOrganizationsInEvents, showOrganizations, repoDescriptions, startupView, largeFonts });
			root.Add(new Section ("Internal") { deleteCache, usage });
			Root = root;

		}
    }
}


