using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels;
using CodeHub.Core.ViewModels.App;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

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


	    protected override void CreateMenuRoot()
		{
            var username = ViewModel.Account.Username;
            var root = new RootElement(username);

            root.Add(new Section
            {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.Execute(null), Images.Person),
                (_notifications = new MenuElement("Notifications", () => ViewModel.GoToNotificationsCommand.Execute(null), Images.Notifications) { NotificationNumber = ViewModel.Notifications }),
                new MenuElement("News", () => ViewModel.GoToNewsComamnd.Execute(null), Images.News),
                new MenuElement("Issues", () => ViewModel.GoToMyIssuesCommand.Execute(null), Images.Flag)
            });

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(username, () => ViewModel.GoToMyEvents.Execute(null), Images.Event));
//            if (ViewModel.Organizations != null && ViewModel.ShowOrganizationsInEvents)
//                ViewModel.Organizations.ForEach(x => eventsSection.Add(new MenuElement(x.Login, () => NavPush(new OrganizationEventsView(username, x.Login)), Images.Event)));
            root.Add(eventsSection);
//
            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
            //repoSection.Add(new MenuElement("Owned", () => NavPush(new UserRepositoriesViewController(Application.Account.Username) { Title = "Owned" }), Images.Repo));
            //repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
            repoSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredRepositoriesCommand.Execute(null), Images.Star));
            //repoSection.Add(new MenuElement("Explore", () => NavPush(new RepositoriesExploreView()), Images.Explore));
            root.Add(repoSection);
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
//                Application.Accounts.ActiveAccount.Organizations.ForEach(x => orgSection.Add(new MenuElement(x.Login, () => NavPush(new OrganizationView(x.Login)), Images.Team)));
//            else
//                orgSection.Add(new MenuElement("Organizations", () => NavPush(new OrganizationsView(username)), Images.Group));
//
//            //There should be atleast 1 thing...
//            if (orgSection.Elements.Count > 0)
//                root.Add(orgSection);
//
            var gistsSection = new Section() { HeaderView = new MenuSectionView("Gists") };
            gistsSection.Add(new MenuElement("My Gists", () => ViewModel.GoToMyGistsCommand.Execute(null), Images.Script));
            gistsSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredGistsCommand.Execute(null), Images.Star2));
            gistsSection.Add(new MenuElement("Public", () => ViewModel.GoToPublicGistsCommand.Execute(null), Images.Public));
            root.Add(gistsSection);
//
            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences".t()) };
            root.Add(infoSection);
            infoSection.Add(new MenuElement("Settings".t(), () => NavPush(new SettingsView()), Images.Cog));
            infoSection.Add(new MenuElement("About".t(), () => NavPush(new AboutViewController()), Images.Info));
            infoSection.Add(new MenuElement("Feedback & Support".t(), PresentUserVoice, Images.Flag));
            infoSection.Add(new MenuElement("Accounts".t(), () => ProfileButtonClicked(this, System.EventArgs.Empty), Images.User));
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

            ViewModel.Bind(x => x.Notifications, x =>
            {
                _notifications.NotificationNumber = x;
                Root.Reload(_notifications, UITableViewRowAnimation.None);
            });

            ViewModel.LoadCommand.Execute(null);


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

