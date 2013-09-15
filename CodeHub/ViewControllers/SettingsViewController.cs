using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using CodeHub.Data;
using CodeFramework.Elements;
using CodeFramework.Controllers;

namespace CodeHub.ViewControllers
{
    public class SettingsViewController : BaseDialogViewController
    {
        public SettingsViewController()
            : base(false)
        {
            Title = "Settings";
            Style = UITableViewStyle.Grouped;
        }

        public override void ViewWillAppear(bool animated)
        {
            var root = new RootElement(Title);
			var currentAccount = Application.Account;

            root.Add(new Section(string.Empty, "If disabled, CodeHub will prompt you for your password when you switch to this account".t()) {
                    new TrueFalseElement("Remember Credentials".t(), !currentAccount.DontRemember, (e) => { 
                        currentAccount.DontRemember = !e.Value; 
                        Application.Accounts.Update(currentAccount);
                    })
            });

            root.Add(new Section(string.Empty, "If enabled, your teams will be shown in the CodeHub slideout menu under Events".t()) {
                new TrueFalseElement("Show Organizations in Events".t(), currentAccount.ShowOrganizationsInEvents, (e) => { 
                    currentAccount.ShowOrganizationsInEvents = e.Value; 
                    Application.Accounts.Update(currentAccount);
                })
            });

            root.Add(new Section(string.Empty, "If enabled, every organization will be listed under Organizations".t()) {
                new TrueFalseElement("List Organizations".t(), currentAccount.ExpandOrganizations, (e) => { 
                    currentAccount.ExpandOrganizations = e.Value; 
                    Application.Accounts.Update(currentAccount);
                })
            });

            root.Add(new Section(string.Empty, "If enabled, repository descriptions will be shown in the list of repositories".t()) {
                new TrueFalseElement("Show Repo Descriptions".t(), currentAccount.ShowRepositoryDescriptionInList, (e) => { 
                    currentAccount.ShowRepositoryDescriptionInList = e.Value; 
                    Application.Accounts.Update(currentAccount);
                })
            });

            root.Add(new Section(string.Empty, "If enabled, send anonymous usage statistics to build a better app".t()) {
                new TrueFalseElement("Send Anonymous Usage".t(), MonoTouch.Utilities.AnalyticsEnabled, (e) => { 
                    MonoTouch.Utilities.AnalyticsEnabled = e.Value;
                })
            });

			//Assign the root
			Root = root;

            base.ViewWillAppear(animated);
        }
    }
}


