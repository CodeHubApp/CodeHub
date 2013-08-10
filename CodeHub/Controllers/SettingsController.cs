using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using CodeHub.Data;
using CodeFramework.Elements;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class SettingsController : BaseDialogViewController
    {
        public SettingsController()
            : base(false)
        {
            Title = "Settings";
            Style = UITableViewStyle.Grouped;
        }

        public override void ViewWillAppear(bool animated)
        {
            var root = new RootElement(Title);
            var currentAccount = Application.Account;

            root.Add(new Section(string.Empty, "If disabled, CodeBucket will prompt you for your password when you switch to this account") {
                new TrueFalseElement("Remember Credentials", !currentAccount.DontRemember, (e) => { 
                    currentAccount.DontRemember = !e.Value; 
                    Application.Accounts.Update(currentAccount);
                })
            });

            root.Add(new Section(string.Empty, "If enabled, your teams will be shown in the CodeBucket slideout menu under Events") {
                new TrueFalseElement("Show Teams in Events", !currentAccount.DontShowTeamEvents, (e) => { 
                    currentAccount.DontShowTeamEvents = !e.Value; 
                    Application.Accounts.Update(currentAccount);
                })
            });

            root.Add(new Section(string.Empty, "If enabled, your teams and groups will be listed under Collaborations") {
                new TrueFalseElement("List Collaborations", !currentAccount.DontExpandTeamsAndGroups, (e) => { 
                    currentAccount.DontExpandTeamsAndGroups = !e.Value; 
                    Application.Accounts.Update(currentAccount);
                })
            });

            root.Add(new Section(string.Empty, "If enabled, send anonymous usage statistics to build a better app") {
                new TrueFalseElement("Send Anonymous Usage", MonoTouch.Utilities.AnalyticsEnabled, (e) => { 
                    MonoTouch.Utilities.AnalyticsEnabled = !e.Value;
                })
            });

            //Assign the root
            Root = root;

            base.ViewWillAppear(animated);
        }
    }
}


