using Cirrious.CrossCore;
using CodeHub.Core.Services;
using MonoTouch.Dialog;
using CodeFramework.iOS.ViewControllers;
using CodeHub.Core.ViewModels.App;

namespace CodeHub.iOS.Views.App
{
	public class SettingsView : ViewModelDrivenViewController
    {
        public SettingsView()
        {
            Title = "Settings";
        }

        public override void ViewWillAppear(bool animated)
        {
            var application = Mvx.Resolve<IApplicationService>();
            var root = new RootElement(Title);
            var currentAccount = application.Account;
			var vm = (SettingsViewModel)ViewModel;

            root.Add(new Section(string.Empty, "If disabled, CodeHub will prompt you for your password when you switch to this account".t()) {
                    new TrueFalseElement("Remember Credentials".t(), !currentAccount.DontRemember, e => { 
                        currentAccount.DontRemember = !e.Value;
                        application.Accounts.Update(currentAccount);
                    })
            });

            root.Add(new Section(string.Empty, "If enabled, your teams will be shown in the CodeHub slideout menu under Events".t()) {
                new TrueFalseElement("Show Organizations in Events".t(), currentAccount.ShowOrganizationsInEvents, e => { 
                    currentAccount.ShowOrganizationsInEvents = e.Value;
                    application.Accounts.Update(currentAccount);
                })
            });

            root.Add(new Section(string.Empty, "If enabled, every organization will be listed under Organizations".t()) {
                new TrueFalseElement("List Organizations".t(), currentAccount.ExpandOrganizations, e => { 
                    currentAccount.ExpandOrganizations = e.Value;
                    application.Accounts.Update(currentAccount);
                })
            });

            root.Add(new Section(string.Empty, "If enabled, repository descriptions will be shown in the list of repositories".t()) {
                new TrueFalseElement("Show Repo Descriptions".t(), currentAccount.ShowRepositoryDescriptionInList, e => { 
                    currentAccount.ShowRepositoryDescriptionInList = e.Value;
                    application.Accounts.Update(currentAccount);
                })
            });

			var el = new StyledStringElement("Startup View", vm.DefaultStartupViewName, MonoTouch.UIKit.UITableViewCellStyle.Value1)
			{ 
				Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator,
			};
			el.Tapped += () => vm.GoToDefaultStartupViewCommand.Execute(null);
            root.Add(new Section(string.Empty, "Select the default startup view after login".t()) { el });
//
//            root.Add(new Section(string.Empty, "If enabled, send anonymous usage statistics to build a better app".t()) {
//                new TrueFalseElement("Send Anonymous Usage".t(), MonoTouch.Utilities.AnalyticsEnabled, e => { 
//                    MonoTouch.Utilities.AnalyticsEnabled = e.Value;
//                })
//            });

//            if (Application.ClientCache != null)
//            {
//                var totalCacheSize = Application.ClientCache.Sum(x => System.IO.File.Exists(x.Path) ? new System.IO.FileInfo(x.Path).Length : 0);
//                var totalCacheSizeMB = ((float)totalCacheSize / 1024f / 1024f).ToString("0.##");
//                var cacheSection = new Section(string.Empty, string.Format("{0} MB of cache".t(), totalCacheSizeMB)); 
//                cacheSection.Add(new StyledStringElement("Delete Cache".t(), () =>
//                { 
//                    Application.ClientCache.DeleteAll();
//                    cacheSection.Footer = string.Format("{0} MB of cache".t(), 0);
//                    ReloadData();
//                }));
//                root.Add(cacheSection);
//            }

			//Assign the root
			Root = root;

            base.ViewWillAppear(animated);
        }
    }
}


