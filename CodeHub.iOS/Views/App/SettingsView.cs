using Cirrious.CrossCore;
using CodeHub.Core.Services;
using MonoTouch.Dialog;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.App;
using CodeFramework.iOS.Utils;
using UIKit;
using Foundation;

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

            accountSection.Add(new TrueFalseElement("Save Credentials", !currentAccount.DontRemember, e =>
            { 
                currentAccount.DontRemember = !e.Value;
                application.Accounts.Update(currentAccount);
            }));

			var showOrganizationsInEvents = new TrueFalseElement("Show Organizations in Events", currentAccount.ShowOrganizationsInEvents, e =>
			{ 
				currentAccount.ShowOrganizationsInEvents = e.Value;
				application.Accounts.Update(currentAccount);
			});

			var showOrganizations = new TrueFalseElement("List Organizations in Menu", currentAccount.ExpandOrganizations, e =>
			{ 
				currentAccount.ExpandOrganizations = e.Value;
				application.Accounts.Update(currentAccount);
			});

			var repoDescriptions = new TrueFalseElement("Show Repo Descriptions", currentAccount.ShowRepositoryDescriptionInList, e =>
			{ 
				currentAccount.ShowRepositoryDescriptionInList = e.Value;
				application.Accounts.Update(currentAccount);
			});

			var startupView = new StyledStringElement("Startup View", vm.DefaultStartupViewName, UIKit.UITableViewCellStyle.Value1)
			{ 
				Accessory = UIKit.UITableViewCellAccessory.DisclosureIndicator,
			};
			startupView.Tapped += () => vm.GoToDefaultStartupViewCommand.Execute(null);

            var largeFonts = new TrueFalseElement("Large Fonts", vm.LargeFonts, x =>
            {
                vm.LargeFonts = x.Value;
                Theme.Setup();
                CreateTable();
            });

            accountSection.Add(new TrueFalseElement("Push Notifications", vm.PushNotificationsEnabled, e => vm.PushNotificationsEnabled = e.Value));
       
            var aboutSection = new Section("About", "Thank you for downloading. Enjoy!")
            {
                new StyledStringElement("Source Code", () => vm.GoToSourceCodeCommand.Execute(null)),
                new StyledStringElement("Follow On Twitter", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp"))),
                new StyledStringElement("Rate This App", () => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8"))),
                new StyledStringElement("App Version", GetApplicationVersion())
            };

			//Assign the root
			var root = new RootElement(Title);
            root.Add(accountSection);
            root.Add(new Section("Appearance") { showOrganizationsInEvents, showOrganizations, repoDescriptions, startupView, largeFonts });
            root.Add(aboutSection);
			Root = root;

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


