using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.Linq;
using System.Threading;
using CodeBucket.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Views;
using CodeHub.GitHub.Controllers.Accounts;
using CodeHub.GitHub.Controllers.Events;
using CodeFramework.Utils;
using CodeHub.GitHub.Controllers.Repositories;
using CodeHub.GitHub.Controllers.Organizations;
using CodeHub.GitHub.Controllers.Gists;
using CodeHub.GitHub.Controllers.Notifications;

namespace CodeHub.Controllers
{
    public class MenuController : MenuBaseController
    {
        protected override void CreateMenuRoot()
        {
            var username = Application.Accounts.ActiveAccount.Username;
            var root = new RootElement(username);
            root.Add(new Section() {
                new MenuElement("Profile", () => NavPush(new ProfileController(username, false) { Title = "Profile" }), Images.Buttons.Person),
                new MenuElement("Notifications", () => NavPush(new NotificationsController()), Images.Buttons.Person),
            });

            var eventsSection = new Section() { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(username, () => NavPush(new EventsController(username)), Images.Buttons.Event));
            if (Application.Accounts.ActiveAccount.Organizations != null && !Application.Accounts.ActiveAccount.DontShowTeamEvents)
                Application.Accounts.ActiveAccount.Organizations.ForEach(x => eventsSection.Add(new MenuElement(x.Login, () => NavPush(new EventsController(x.Login)), Images.Buttons.Event)));
            root.Add(eventsSection);

            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
            repoSection.Add(new MenuElement("Owned", () => NavPush(new RepositoryController(Application.Accounts.ActiveAccount.Username) { Title = "Owned" }), Images.Repo));
            repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
            repoSection.Add(new MenuElement("Starred", () => NavPush(new StarredRepositoryController()), Images.Heart));
            repoSection.Add(new MenuElement("Explore", () => NavPush(new ExploreController()), Images.Buttons.Explore));
            root.Add(repoSection);

            var groupsTeamsSection = new Section() { HeaderView = new MenuSectionView("Organizations") };
            if (Application.Accounts.ActiveAccount.Organizations != null)
                Application.Accounts.ActiveAccount.Organizations.ForEach(x => groupsTeamsSection.Add(new MenuElement(x.Login, () => NavPush(new OrganizationInfoController(x.Login)), Images.Team)));

            //There should be atleast 1 thing...
            if (groupsTeamsSection.Elements.Count > 0)
                root.Add(groupsTeamsSection);


            var gistsSection = new Section() { HeaderView = new MenuSectionView("Gists") };
            gistsSection.Add(new MenuElement("My Gists", () => NavPush(new AccountGistsController(Application.Accounts.ActiveAccount.Username)), Images.Repo));
            gistsSection.Add(new MenuElement("Starred", () => NavPush(new StarredGistsController()), Images.RepoFollow));
            gistsSection.Add(new MenuElement("Public", () => NavPush(new PublicGistsController()), Images.Heart));
            root.Add(gistsSection);


            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences") };
            root.Add(infoSection);
            infoSection.Add(new MenuElement("Settings", () => NavPush(new SettingsController()), Images.Buttons.Cog));
            infoSection.Add(new MenuElement("About", () => NavPush(new AboutController()), Images.Buttons.Info));
            infoSection.Add(new MenuElement("Feedback & Support", PresentUserVoice, Images.Buttons.Flag));
            infoSection.Add(new MenuElement("Accounts", () => ProfileButtonClicked(this, System.EventArgs.Empty), Images.Buttons.User));
            Root = root;
        }

        private void PresentUserVoice()
        {
            var config = UserVoice.UVConfig.Create("http://codebucket.uservoice.com", "pnuDmPENErDiDpXrms1DTg", "iDboMdCIwe2E5hJFa8hy9K9I5wZqnjKCE0RPHLhZIk");
            UserVoice.UserVoice.PresentUserVoiceInterface(this, config);
        }

        protected override void ProfileButtonClicked(object sender, System.EventArgs e)
        {
            var accounts = new AccountsController();
            var nav = new UINavigationController(accounts);
            accounts.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(CodeFramework.Images.Buttons.Cancel, () => {
                var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
                Transitions.Transition(appDelegate.Slideout, UIViewAnimationOptions.TransitionFlipFromRight);
            }));
            Transitions.Transition(nav, UIViewAnimationOptions.TransitionFlipFromLeft);
        }

        public override void ViewWillAppear(bool animated)
        {
            ProfileButton.Uri = new System.Uri(Application.Accounts.ActiveAccount.AvatarUrl);

            //This must be last.
            base.ViewWillAppear(animated);
        }
    }
}
