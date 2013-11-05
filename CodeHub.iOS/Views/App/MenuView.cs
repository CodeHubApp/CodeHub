using CodeFramework.iOS.ViewControllers;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.App
{
	public class MenuView : MenuBaseViewController
    {
        private MenuElement _notifications;

	    public new MenuViewModel ViewModel
	    {
	        get { return (MenuViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
	    }

//	    public override void ViewDidAppear(bool animated)
//	    {
//	        base.ViewDidAppear(animated);
//            ViewModel.GoToDefaultTopView.Execute(null);
//	    }

	    protected override void CreateMenuRoot()
		{
            var username = ViewModel.Account.Username;
            var root = new RootElement(username);

            root.Add(new Section() {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.Execute(null), Images.Person)
                //(_notifications = new MenuElement("Notifications", () => NavPush(new NotificationsViewController()), Images.Notifications)),
                //new MenuElement("News", () => NavPush(new NewsViewController()), Images.News),
                //new MenuElement("Issues", () => NavPush(new MyIssuesViewController()), Images.Flag)
            });
//
//            if (Application.Account.Notifications != null)
//                _notifications.NotificationNumber = Application.Account.Notifications.Value;
//
//            var eventsSection = new Section() { HeaderView = new MenuSectionView("Events") };
//            eventsSection.Add(new MenuElement(Application.Account.Username, () => NavPush(new UserEventsViewController(Application.Account.Username)), Images.Event));
//            if (Application.Account.Organizations != null && Application.Account.ShowOrganizationsInEvents)
//                Application.Account.Organizations.ForEach(x => eventsSection.Add(new MenuElement(x.Login, () => NavPush(new OrganizationEventsViewController(username, x.Login)), Images.Event)));
//            root.Add(eventsSection);
//
//            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
//            repoSection.Add(new MenuElement("Owned", () => NavPush(new UserRepositoriesViewController(Application.Account.Username) { Title = "Owned" }), Images.Repo));
//            //repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
//            repoSection.Add(new MenuElement("Starred", () => NavPush(new RepositoriesStarredViewController()), Images.Star));
//            repoSection.Add(new MenuElement("Explore", () => NavPush(new RepositoriesExploreViewController()), Images.Explore));
//            root.Add(repoSection);
//            
//            var pinnedRepos = Application.Account.GetPinnedRepositories();
//            if (pinnedRepos.Count > 0)
//            {
//                var pinnedRepoSection = new Section() { HeaderView = new MenuSectionView("Favorite Repositories".t()) };
//                pinnedRepos.ForEach(x => pinnedRepoSection.Add(new MenuElement(x.Name, () => NavPush(new RepositoryViewController(x.Owner, x.Slug, x.Name)), Images.Repo) { ImageUri = new System.Uri(x.ImageUri) }));
//                root.Add(pinnedRepoSection);
//            }
//
//            var orgSection = new Section() { HeaderView = new MenuSectionView("Organizations") };
//            if (Application.Accounts.ActiveAccount.Organizations != null && Application.Account.ExpandOrganizations)
//                Application.Accounts.ActiveAccount.Organizations.ForEach(x => orgSection.Add(new MenuElement(x.Login, () => NavPush(new OrganizationViewController(x.Login)), Images.Team)));
//            else
//                orgSection.Add(new MenuElement("Organizations", () => NavPush(new OrganizationsViewController(username)), Images.Group));
//
//            //There should be atleast 1 thing...
//            if (orgSection.Elements.Count > 0)
//                root.Add(orgSection);
//
//            var gistsSection = new Section() { HeaderView = new MenuSectionView("Gists") };
//            gistsSection.Add(new MenuElement("My Gists", () => NavPush(new AccountGistsViewController(Application.Account.Username)), Images.Script));
//            gistsSection.Add(new MenuElement("Starred", () => NavPush(new StarredGistsViewController()), Images.Star2));
//            gistsSection.Add(new MenuElement("Public", () => NavPush(new PublicGistsViewController()), Images.Public));
//            root.Add(gistsSection);
//
//            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences".t()) };
//            root.Add(infoSection);
//            infoSection.Add(new MenuElement("Settings".t(), () => NavPush(new SettingsViewController()), Images.Cog));
//            infoSection.Add(new MenuElement("About".t(), () => NavPush(new AboutViewController()), Images.Info));
//            infoSection.Add(new MenuElement("Feedback & Support".t(), PresentUserVoice, Images.Flag));
//            infoSection.Add(new MenuElement("Accounts".t(), () => ProfileButtonClicked(this, System.EventArgs.Empty), Images.User));
            Root = root;
		}

        private void PresentUserVoice()
        {
//            var config = UserVoice.UVConfig.Create("http://codehub.uservoice.com", "95D8N9Q3UT1Asn89F7d3lA", "xptp5xR6RtqTPpcopKrmOFWVQ4AIJEvr2LKx6KFGgE4");
//            UserVoice.UserVoice.PresentUserVoiceInterface(this, config);
        }

        protected override void ProfileButtonClicked(object sender, System.EventArgs e)
        {
            ViewModel.GoToAccountsCommand.Execute(null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
//
//            ProfileButton.Uri = new System.Uri(Application.Account.AvatarUrl);
//
//            if (Application.Account.Notifications == null)
//            {
//                this.DoWorkNoHud(() =>
//                {
//                    //Don't bother saving the result. This get's cached in memory so there's no reason to save it twice. Just save the number of entires
//                    var notificationRequest = Application.Client.Notifications.GetAll();
//                    notificationRequest.CheckIfModified = notificationRequest.RequestFromCache = false;
//
//
//                    Application.Account.Notifications = Application.Client.Execute(notificationRequest).Data.Count;
//
//                    if (_notifications != null)
//                    {
//                        _notifications.NotificationNumber = Application.Account.Notifications.Value;
//                        BeginInvokeOnMainThread(() => Root.Reload(_notifications, UITableViewRowAnimation.None));
//                    }
//                });
//            }
//
//            if (Application.Account.Organizations == null)
//            {
//                this.DoWorkNoHud(() =>
//                {
//                    var organizationRequest = Application.Client.AuthenticatedUser.GetOrganizations();
//                    organizationRequest.CheckIfModified = organizationRequest.RequestFromCache = false;
//
//                    Application.Account.Organizations = Application.Client.Execute(organizationRequest).Data;
//                    BeginInvokeOnMainThread(() => CreateMenuRoot());
//                });
//            }
        }

    }
}

