using System;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.Utilities;
using UIKit;
using Foundation;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.App
{
	public class SettingsView : ViewModelDrivenDialogViewController
    {
        public SettingsView()
        {
            Title = "Settings";

            OnActivation(d =>
            {
                var vm = (SettingsViewModel)ViewModel;
                d(vm.Bind(x => x.PushNotificationsEnabled).Subscribe(_ => CreateTable()));
                d(vm.Bind(x => x.IsSaving).SubscribeStatus("Saving..."));
                CreateTable();
            });
        }

		private void CreateTable()
		{
			var application = Mvx.Resolve<IApplicationService>();
			var vm = (SettingsViewModel)ViewModel;
			var currentAccount = application.Account;
            var accountSection = new Section("Account");

            var saveCredentials = new BooleanElement("Save Credentials", !currentAccount.DontRemember);
            saveCredentials.Changed.Subscribe(x => { 
                currentAccount.DontRemember = !x;
                application.Accounts.Update(currentAccount);
            });
            accountSection.Add(saveCredentials);

            var showOrganizationsInEvents = new BooleanElement("Show Organizations in Events", currentAccount.ShowOrganizationsInEvents);
            showOrganizationsInEvents.Changed.Subscribe(x => {
				currentAccount.ShowOrganizationsInEvents = x;
				application.Accounts.Update(currentAccount);
			});

            var showOrganizations = new BooleanElement("List Organizations in Menu", currentAccount.ExpandOrganizations);
            showOrganizations.Changed.Subscribe(x => { 
				currentAccount.ExpandOrganizations = x;
				application.Accounts.Update(currentAccount);
			});

            var repoDescriptions = new BooleanElement("Show Repo Descriptions", currentAccount.ShowRepositoryDescriptionInList);
            repoDescriptions.Changed.Subscribe(x => {
				currentAccount.ShowRepositoryDescriptionInList = x;
				application.Accounts.Update(currentAccount);
			});

			var startupView = new StringElement("Startup View", vm.DefaultStartupViewName, UITableViewCellStyle.Value1)
			{ 
				Accessory = UITableViewCellAccessory.DisclosureIndicator,
			};
            startupView.Clicked.BindCommand(vm.GoToDefaultStartupViewCommand);

            var pushNotifications = new BooleanElement("Push Notifications", vm.PushNotificationsEnabled);
            pushNotifications.Changed.Subscribe(e => vm.PushNotificationsEnabled = e);
            accountSection.Add(pushNotifications);
       
            var source = new StringElement("Source Code");
            source.Clicked.BindCommand(vm.GoToSourceCodeCommand);

            var follow = new StringElement("Follow On Twitter");
            follow.Clicked.Subscribe(_ => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp")));

            var rate = new StringElement("Rate This App");
            rate.Clicked.Subscribe(_ => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8")));

            var aboutSection = new Section("About", "Thank you for downloading. Enjoy!") { source, follow, rate };
        
            if (vm.ShouldShowUpgrades)
            {
                var upgrades = new StringElement("Upgrades");
                upgrades.Clicked.BindCommand(vm.GoToUpgradesCommand);
                aboutSection.Add(upgrades);
            }

            var appVersion = new StringElement("App Version", GetApplicationVersion())
            { 
                Accessory = UITableViewCellAccessory.None,
                SelectionStyle = UITableViewCellSelectionStyle.None
            };

            aboutSection.Add(appVersion);

			//Assign the root
            Root.Reset(accountSection, new Section("Appearance") { showOrganizationsInEvents, showOrganizations, repoDescriptions, startupView }, aboutSection);
		}

        private static string GetApplicationVersion() 
        {
            string shortVersion = string.Empty;
            string bundleVersion = string.Empty;

            try
            {
                shortVersion = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            }
            catch { }

            try
            {
                bundleVersion = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
            }
            catch { }

            if (string.Equals(shortVersion, bundleVersion))
                return shortVersion;

            return string.IsNullOrEmpty(bundleVersion) ? shortVersion : string.Format("{0} ({1})", shortVersion, bundleVersion);
        }
    }
}


