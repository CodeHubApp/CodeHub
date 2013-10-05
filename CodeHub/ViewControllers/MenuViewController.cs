using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.Linq;
using System.Threading;
using CodeFramework.Controllers;
using CodeFramework.Views;
using CodeFramework.Utils;
using System.Collections.Generic;

namespace CodeHub.ViewControllers
{
	public class MenuViewController : MenuBaseViewController
    {
        private MenuElement _notifications;

		protected override void CreateMenuRoot()
		{
            var username = Application.Account.Username;
            var root = new RootElement(username);

            root.Add(new Section() {
                new MenuElement("Profile", () => NavPush(new ProfileViewController(username) { Title = "Profile" }), Images.Person),
                (_notifications = new MenuElement("Notifications", () => NavPush(new NotificationsViewController()), Images.Notifications) { NotificationNumber = Application.Account.Notifications }),
                new MenuElement("News", () => NavPush(new NewsViewController()), Images.News),
                new MenuElement("Issues", () => NavPush(new MyIssuesViewController()), Images.Flag)
            });

            var eventsSection = new Section() { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(Application.Account.Username, () => NavPush(new EventsViewController(Application.Account.Username)), Images.Event));
            if (Application.Account.Organizations != null && Application.Account.ShowOrganizationsInEvents)
                Application.Account.Organizations.ForEach(x => eventsSection.Add(new MenuElement(x.Login, () => NavPush(new OrganizationEventsViewController(username, x.Login)), Images.Event)));
            root.Add(eventsSection);

            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
            repoSection.Add(new MenuElement("Owned", () => NavPush(new RepositoriesViewController(Application.Account.Username) { Title = "Owned" }), Images.Repo));
            //repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
            repoSection.Add(new MenuElement("Starred", () => NavPush(new RepositoriesStarredViewController()), Images.Star));
            repoSection.Add(new MenuElement("Explore", () => NavPush(new RepositoriesExploreViewController()), Images.Explore));
            root.Add(repoSection);
            
            var pinnedRepos = Application.Account.GetPinnedRepositories();
            if (pinnedRepos.Count > 0)
            {
                var pinnedRepoSection = new Section() { HeaderView = new MenuSectionView("Favorite Repositories".t()) };
                pinnedRepos.ForEach(x => pinnedRepoSection.Add(new MenuElement(x.Name, () => NavPush(new RepositoryInfoViewController(x.Owner, x.Slug, x.Name)), Images.Repo) { ImageUri = new System.Uri(x.ImageUri) }));
                root.Add(pinnedRepoSection);
            }

            var orgSection = new Section() { HeaderView = new MenuSectionView("Organizations") };
            if (Application.Accounts.ActiveAccount.Organizations != null && Application.Account.ExpandOrganizations)
                Application.Accounts.ActiveAccount.Organizations.ForEach(x => orgSection.Add(new MenuElement(x.Login, () => NavPush(new OrganizationViewController(x.Login)), Images.Team)));
            else
                orgSection.Add(new MenuElement("Organizations", () => NavPush(new OrganizationsViewController(username)), Images.Group));

            //There should be atleast 1 thing...
            if (orgSection.Elements.Count > 0)
                root.Add(orgSection);

            var gistsSection = new Section() { HeaderView = new MenuSectionView("Gists") };
            gistsSection.Add(new MenuElement("My Gists", () => NavPush(new AccountGistsViewController(Application.Account.Username)), Images.Script));
            gistsSection.Add(new MenuElement("Starred", () => NavPush(new StarredGistsViewController()), Images.Star2));
            gistsSection.Add(new MenuElement("Public", () => NavPush(new PublicGistsViewController()), Images.Public));
            root.Add(gistsSection);

            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences".t()) };
            root.Add(infoSection);
            infoSection.Add(new MenuElement("Settings".t(), () => NavPush(new SettingsViewController()), Images.Cog));
            infoSection.Add(new MenuElement("About".t(), () => NavPush(new AboutViewController()), Images.Info));
            infoSection.Add(new MenuElement("Feedback & Support".t(), PresentUserVoice, Images.Flag));
            infoSection.Add(new MenuElement("Accounts".t(), () => ProfileButtonClicked(this, System.EventArgs.Empty), Images.User));
            Root = root;
		}

        private void PresentUserVoice()
        {
            var config = UserVoice.UVConfig.Create("http://codehub.uservoice.com", "95D8N9Q3UT1Asn89F7d3lA", "xptp5xR6RtqTPpcopKrmOFWVQ4AIJEvr2LKx6KFGgE4");
            UserVoice.UserVoice.PresentUserVoiceInterface(this, config);
        }

        protected override void ProfileButtonClicked(object sender, System.EventArgs e)
        {
            var accounts = new AccountsViewController();
            var nav = new UINavigationController(accounts);
            accounts.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.CancelButton, () => {
                var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
                Transitions.Transition(appDelegate.Slideout, UIViewAnimationOptions.TransitionFlipFromRight);
            }));
            Transitions.Transition(nav, UIViewAnimationOptions.TransitionFlipFromLeft);
        }

        private bool _extrasLoaded = false;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (_extrasLoaded == false)
            {
                LoadExtras();
                _extrasLoaded = true;
            }
        }

        private void LoadExtras()
        {
            ProfileButton.Uri = new System.Uri(Application.Account.AvatarUrl);

            this.DoWorkNoHud(() => {
                //Don't bother saving the result. This get's cached in memory so there's no reason to save it twice. Just save the number of entires
                Application.Account.Notifications = Application.Client.Execute(Application.Client.Notifications.GetAll()).Data.Count;
                _notifications.NotificationNumber = Application.Account.Notifications;
                InvokeOnMainThread(() => Root.Reload(_notifications, UITableViewRowAnimation.None));
            });

            this.DoWorkNoHud(() => {
                Application.Account.Organizations = Application.Client.Execute(Application.Client.AuthenticatedUser.GetOrganizations()).Data;
                InvokeOnMainThread(() => CreateMenuRoot());
            });
        }
    }
}

