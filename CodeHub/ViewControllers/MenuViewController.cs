using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.Linq;
using System.Threading;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Views;
using CodeFramework.Utils;
using System.Collections.Generic;

namespace CodeHub.ViewControllers
{
	public class MenuViewController : MenuBaseController
    {
		protected override void CreateMenuRoot()
		{
            var username = Application.Account.Username;
            var root = new RootElement(username);
            root.Add(new Section() {
                new MenuElement("Profile", () => NavPush(new ProfileViewController(username) { Title = "Profile" }), Images.Buttons.Person),
                new MenuElement("Notifications", () => NavPush(new NotificationsViewController()), Images.CommentAdd)
            });

            var eventsSection = new Section() { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(Application.Account.Username, () => NavPush(new EventsViewController(Application.Account.Username)), Images.Buttons.Event));
            if (Application.Accounts.ActiveAccount.Organizations != null && !Application.Accounts.ActiveAccount.DontShowTeamEvents)
                Application.Accounts.ActiveAccount.Organizations.ForEach(x => eventsSection.Add(new MenuElement(x.Login, () => NavPush(new EventsViewController(x.Login)), Images.Buttons.Event)));
            root.Add(eventsSection);

            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
            repoSection.Add(new MenuElement("Owned", () => NavPush(new RepositoriesViewController(Application.Accounts.ActiveAccount.Username) { Title = "Owned" }), Images.Repo));
            //repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
            repoSection.Add(new MenuElement("Starred", () => NavPush(new RepositoriesStarredViewController()), Images.Heart));
            repoSection.Add(new MenuElement("Explore", () => NavPush(new RepositoriesExploreViewController()), Images.Buttons.Explore));
            root.Add(repoSection);
            
            var pinnedRepos = Application.Account.GetPinnedRepositories();
            if (pinnedRepos.Count > 0)
            {
                var pinnedRepoSection = new Section() { HeaderView = new MenuSectionView("Favorite Repositories".t()) };
                pinnedRepos.ForEach(x => pinnedRepoSection.Add(new MenuElement(x.Name, () => NavPush(new RepositoryInfoViewController(x.Owner, x.Slug, x.Name)), Images.Repo) { ImageUri = new System.Uri(x.ImageUri) }));
                root.Add(pinnedRepoSection);
            }

            var groupsTeamsSection = new Section() { HeaderView = new MenuSectionView("Organizations") };
            //if (Application.Accounts.ActiveAccount.Organizations != null)
                //Application.Accounts.ActiveAccount.Organizations.ForEach(x => groupsTeamsSection.Add(new MenuElement(x.Login, () => NavPush(new OrganizationInfoViewController(x.Login)), Images.Team)));

            //There should be atleast 1 thing...
            if (groupsTeamsSection.Elements.Count > 0)
                root.Add(groupsTeamsSection);

            var gistsSection = new Section() { HeaderView = new MenuSectionView("Gists") };
            gistsSection.Add(new MenuElement("My Gists", () => NavPush(new AccountGistsViewController(Application.Accounts.ActiveAccount.Username)), Images.Repo));
            gistsSection.Add(new MenuElement("Starred", () => NavPush(new StarredGistsViewController()), Images.RepoFollow));
            gistsSection.Add(new MenuElement("Public", () => NavPush(new PublicGistsViewController()), Images.Heart));
            root.Add(gistsSection);

            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences".t()) };
            root.Add(infoSection);
            infoSection.Add(new MenuElement("Settings".t(), () => NavPush(new SettingsViewController()), Images.Buttons.Cog));
            infoSection.Add(new MenuElement("About".t(), () => NavPush(new AboutViewController()), Images.Buttons.Info));
            infoSection.Add(new MenuElement("Feedback & Support".t(), PresentUserVoice, Images.Buttons.Flag));
            infoSection.Add(new MenuElement("Accounts".t(), () => ProfileButtonClicked(this, System.EventArgs.Empty), Images.Buttons.User));
            Root = root;
		}

        private void PresentUserVoice()
        {
            var config = UserVoice.UVConfig.Create("http://CodeHub.uservoice.com", "pnuDmPENErDiDpXrms1DTg", "iDboMdCIwe2E5hJFa8hy9K9I5wZqnjKCE0RPHLhZIk");
            UserVoice.UserVoice.PresentUserVoiceInterface(this, config);
        }

        protected override void ProfileButtonClicked(object sender, System.EventArgs e)
        {
            var accounts = new AccountsViewController();
            var nav = new UINavigationController(accounts);
            accounts.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(CodeFramework.Images.Buttons.Cancel, () => {
                var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
                Transitions.Transition(appDelegate.Slideout, UIViewAnimationOptions.TransitionFlipFromRight);
            }));
            Transitions.Transition(nav, UIViewAnimationOptions.TransitionFlipFromLeft);
        }

        public override void ViewDidLoad()
        {
            ProfileButton.Uri = new System.Uri(Application.Account.AvatarUrl);

            //Must be in the middle
            base.ViewDidLoad();

            //Load optional stuff
            LoadExtras();
        }

        private void LoadExtras()
        {

        }
    }
}

