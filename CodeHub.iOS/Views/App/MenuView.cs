using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.App;
using UIKit;
using System.Linq;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;
using System;
using MvvmCross.Platform;
using CodeHub.iOS.DialogElements;
using System.Collections.Generic;
using CodeHub.iOS.ViewControllers.Accounts;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CodeHub.iOS.Views.App
{
    public class MenuView : MenuBaseViewController
    {
        private MenuElement _notifications;
        private Section _favoriteRepoSection;

        public new MenuViewModel ViewModel
        {
            get { return (MenuViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public MenuView()
        {
            var appService = Mvx.Resolve<IApplicationService>();
            var featuresService = Mvx.Resolve<IFeaturesService>();
            ViewModel = new MenuViewModel(appService, featuresService);
            Appeared.Take(1).Subscribe(_ => PromptPushNotifications());
        }

        private static async Task PromptPushNotifications()
        {
            var appService = Mvx.Resolve<IApplicationService>();
            if (appService.Account.IsEnterprise)
                return;

            var featuresService = Mvx.Resolve<IFeaturesService>();
            if (!featuresService.IsPushNotificationsActivated)
                return;

            var alertDialogService = Mvx.Resolve<IAlertDialogService>();
            var pushNotifications = Mvx.Resolve<IPushNotificationsService>();

            if (appService.Account.IsPushNotificationsEnabled == null)
            {
                var result = await alertDialogService.PromptYesNo("Push Notifications", "Would you like to enable push notifications for this account?");
                appService.Account.IsPushNotificationsEnabled = result;
                appService.Accounts.Update(appService.Account);

                if (result)
                {
                    pushNotifications.Register().ToBackground();
                }

            }
            else if (appService.Account.IsPushNotificationsEnabled.Value)
            {
                pushNotifications.Register().ToBackground();
            }
        }

        protected override void CreateMenuRoot()
        {
            var username = ViewModel.Account.Username;
            Title = username;
            ICollection<Section> sections = new LinkedList<Section>();

            sections.Add(new Section
            {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.Execute(null), Octicon.Person.ToImage()),
                (_notifications = new MenuElement("Notifications", () => ViewModel.GoToNotificationsCommand.Execute(null), Octicon.Inbox.ToImage()) { NotificationNumber = ViewModel.Notifications }),
                new MenuElement("News", () => ViewModel.GoToNewsCommand.Execute(null), Octicon.RadioTower.ToImage()),
                new MenuElement("Issues", () => ViewModel.GoToMyIssuesCommand.Execute(null), Octicon.IssueOpened.ToImage())
            });

            Uri avatarUri;
            Uri.TryCreate(ViewModel.Account.AvatarUrl, UriKind.Absolute, out avatarUri);

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(username, () => ViewModel.GoToMyEvents.Execute(null), Octicon.Rss.ToImage(), avatarUri));
            if (ViewModel.Organizations != null && ViewModel.Account.ShowOrganizationsInEvents)
            {
                foreach (var org in ViewModel.Organizations)
                {
                    Uri.TryCreate(org.AvatarUrl, UriKind.Absolute, out avatarUri);
                    eventsSection.Add(new MenuElement(org.Login, () => ViewModel.GoToOrganizationEventsCommand.Execute(org.Login), Octicon.Rss.ToImage(), avatarUri));
                }
            }
            sections.Add(eventsSection);

            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
            repoSection.Add(new MenuElement("Owned", () => ViewModel.GoToOwnedRepositoriesCommand.Execute(null), Octicon.Repo.ToImage()));
            repoSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredRepositoriesCommand.Execute(null), Octicon.Star.ToImage()));
            repoSection.Add(new MenuElement("Trending", () => ViewModel.GoToTrendingRepositoriesCommand.Execute(null), Octicon.Pulse.ToImage()));
            repoSection.Add(new MenuElement("Explore", () => ViewModel.GoToExploreRepositoriesCommand.Execute(null), Octicon.Globe.ToImage()));
            sections.Add(repoSection);
            
            if (ViewModel.PinnedRepositories.Any())
            {
                _favoriteRepoSection = new Section() { HeaderView = new MenuSectionView("Favorite Repositories") };
                foreach (var pinnedRepository in ViewModel.PinnedRepositories)
                    _favoriteRepoSection.Add(new PinnedRepoElement(pinnedRepository, ViewModel.GoToRepositoryCommand));
                sections.Add(_favoriteRepoSection);
            }
            else
            {
                _favoriteRepoSection = null;
            }

            var orgSection = new Section() { HeaderView = new MenuSectionView("Organizations") };
            if (ViewModel.Organizations != null && ViewModel.Account.ExpandOrganizations)
            {
                foreach (var org in ViewModel.Organizations)
                {
                    Uri.TryCreate(org.AvatarUrl, UriKind.Absolute, out avatarUri);
                    orgSection.Add(new MenuElement(org.Login, () => ViewModel.GoToOrganizationCommand.Execute(org.Login), Images.Avatar, avatarUri));
                }
            }
            else
                orgSection.Add(new MenuElement("Organizations", () => ViewModel.GoToOrganizationsCommand.Execute(null), Octicon.Organization.ToImage()));

            //There should be atleast 1 thing...
            if (orgSection.Elements.Count > 0)
                sections.Add(orgSection);

            var gistsSection = new Section() { HeaderView = new MenuSectionView("Gists") };
            gistsSection.Add(new MenuElement("My Gists", () => ViewModel.GoToMyGistsCommand.Execute(null), Octicon.Gist.ToImage()));
            gistsSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredGistsCommand.Execute(null), Octicon.Star.ToImage()));
            gistsSection.Add(new MenuElement("Public", () => ViewModel.GoToPublicGistsCommand.Execute(null), Octicon.Globe.ToImage()));
            sections.Add(gistsSection);
//
            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences") };
            sections.Add(infoSection);
            infoSection.Add(new MenuElement("Settings", () => ViewModel.GoToSettingsCommand.Execute(null), Octicon.Gear.ToImage()));

            if (ViewModel.ShouldShowUpgrades)
                infoSection.Add(new MenuElement("Upgrades", () => ViewModel.GoToUpgradesCommand.Execute(null), Octicon.Lock.ToImage()));
            
            infoSection.Add(new MenuElement("Feedback & Support", PresentUserVoice, Octicon.CommentDiscussion.ToImage()));
            infoSection.Add(new MenuElement("Accounts", ProfileButtonClicked, Octicon.Person.ToImage()));

            Root.Reset(sections);
        }


        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            #if DEBUG
            GC.Collect();
            GC.Collect();
            GC.Collect();
            #endif
        }

        private void PresentUserVoice()
        {
            ViewModel.GoToSupport.Execute(null);
        }

        protected override void ProfileButtonClicked()
        {
            var vc = new AccountsViewController();
            vc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Buttons.CancelButton };
            vc.NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => DismissViewController(true, null);
            PresentViewController(new ThemedNavigationController(vc), true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.SeparatorInset = UIEdgeInsets.Zero;
            TableView.SeparatorColor = UIColor.FromRGB(50, 50, 50);

            if (!string.IsNullOrEmpty(ViewModel.Account.AvatarUrl))
                ProfileButton.Uri = new Uri(ViewModel.Account.AvatarUrl);

            ViewModel.Bind(x => x.Notifications).Subscribe(x =>
            {
                if (_notifications != null)
                {
                    _notifications.NotificationNumber = x;
                }
            });

            ViewModel.Bind(x => x.Organizations).Subscribe(x => CreateMenuRoot());

            ViewModel.LoadCommand.Execute(null);

            var appService = Mvx.Resolve<IApplicationService> ();

            // A user has been activated!
            if (appService.ActivationAction != null)
            {
                appService.ActivationAction();
                appService.ActivationAction = null;
            }
        }

        private class PinnedRepoElement : MenuElement
        {
            public CodeHub.Core.Data.PinnedRepository PinnedRepo
            {
                get;
                private set; 
            }

            public PinnedRepoElement(CodeHub.Core.Data.PinnedRepository pinnedRepo, System.Windows.Input.ICommand command)
                : base(pinnedRepo.Name, () => command.Execute(new RepositoryIdentifier { Owner = pinnedRepo.Owner, Name = pinnedRepo.Name }), Octicon.Repo.ToImage())
            {
                PinnedRepo = pinnedRepo;

                // BUG FIX: App keeps getting relocated so the URLs become off
                if (new [] { "repository.png", "repository_fork.png" }.Any(x => PinnedRepo.ImageUri.EndsWith(x, StringComparison.Ordinal)))
                {
                    ImageUri = new Uri("http://codehub-app.com/assets/repository_icon.png");
                }
                else
                {
                    ImageUri = new Uri(PinnedRepo.ImageUri);
                }
            }
        }

        private void DeletePinnedRepo(PinnedRepoElement el)
        {
            ViewModel.DeletePinnedRepositoryCommand.Execute(el.PinnedRepo);

            if (_favoriteRepoSection.Elements.Count == 1)
            {
                Root.Remove(_favoriteRepoSection);
                _favoriteRepoSection = null;
            }
            else
            {
                _favoriteRepoSection.Remove(el);
            }
        }

        public override DialogViewController.Source CreateSizingSource()
        {
            return new EditSource(this);
        }

        private class EditSource : Source
        {
            private readonly WeakReference<MenuView> _parent;
            public EditSource(MenuView dvc) 
                : base (dvc)
            {
                _parent = new WeakReference<MenuView>(dvc);
            }

            public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                var view = _parent.Get();
                if (view == null)
                    return false;

                if (view._favoriteRepoSection == null)
                    return false;
                if (view.Root[indexPath.Section] == view._favoriteRepoSection)
                    return true;
                return false;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                var view = _parent.Get();
                if (view == null)
                    return UITableViewCellEditingStyle.None;

                if (view._favoriteRepoSection != null && view.Root[indexPath.Section] == view._favoriteRepoSection)
                    return UITableViewCellEditingStyle.Delete;
                return UITableViewCellEditingStyle.None;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
            {
                var view = _parent.Get();
                if (view == null)
                    return;
                
                switch (editingStyle)
                {
                    case UITableViewCellEditingStyle.Delete:
                        var section = view.Root[indexPath.Section];
                        var element = section[indexPath.Row];
                        view.DeletePinnedRepo(element as PinnedRepoElement);
                        break;
                }
            }
        }
    }
}

