using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Accounts;
using CodeHub.iOS.ViewControllers.Search;
using CodeHub.iOS.ViewControllers.Settings;
using CodeHub.iOS.Views;
using CoreGraphics;
using MvvmCross.Platform;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class MenuViewController : ViewModelDrivenDialogViewController
    {
        private readonly ProfileButton _profileButton = new ProfileButton();
        private readonly UILabel _title;
        private MenuElement _notifications;
        private Section _favoriteRepoSection;

        public new MenuViewModel ViewModel
        {
            get { return (MenuViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        private static bool IsAccountEnterprise
            => Locator.Current.GetService<IApplicationService>().Account?.IsEnterprise ?? false;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public override string Title {
            get {
                return _title == null ? base.Title : " " + _title.Text;
            }
            set {
                if (_title != null)
                    _title.Text = " " + value;
                base.Title = value;
            }
        }

        public MenuViewController()
            : base(false, UITableViewStyle.Plain)
        {
            ViewModel = new MenuViewModel();
            Appeared.Take(1).Subscribe(_ => PromptPushNotifications());

            _title = new UILabel(new CGRect(0, 40, 320, 40));
            _title.TextAlignment = UITextAlignment.Left;
            _title.BackgroundColor = UIColor.Clear;
            _title.Font = UIFont.SystemFontOfSize(16f);
            _title.TextColor = UIColor.FromRGB(246, 246, 246);
            NavigationItem.TitleView = _title;

            OnActivation(d =>
            {
                d(_profileButton.GetClickedObservable().Subscribe(_ => ProfileButtonClicked()));
            });
        }

        private static async Task PromptPushNotifications()
        {
            var appService = Mvx.Resolve<IApplicationService>();
            if (IsAccountEnterprise)
                return;

            var featuresService = Mvx.Resolve<IFeaturesService>();
            if (!featuresService.IsProEnabled)
                return;

            var alertDialogService = Mvx.Resolve<IAlertDialogService>();
            var pushNotifications = Mvx.Resolve<IPushNotificationsService>();

            if (appService.Account.IsPushNotificationsEnabled == null)
            {
                var result = await alertDialogService.PromptYesNo("Push Notifications", "Would you like to enable push notifications for this account?");
                var accountsService = Mvx.Resolve<IAccountsService>();
                appService.Account.IsPushNotificationsEnabled = result;
                await accountsService.Save(appService.Account);

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


        private void UpdateProfilePicture()
        {
            var size = new CGSize(32, 32);
            if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft ||
                UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight)
            {
                size = new CGSize(24, 24);
            }

            _profileButton.Frame = new CGRect(new CGPoint(0, 4), size);

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(_profileButton);
        }

        private void CreateMenuRoot()
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
            repoSection.Add(new MenuElement("Owned", GoToOwnedRepositories, Octicon.Repo.ToImage()));
            repoSection.Add(new MenuElement("Starred", GoToStarredRepositories, Octicon.Star.ToImage()));
            repoSection.Add(new MenuElement("Trending", GoToTrendingRepositories, Octicon.Pulse.ToImage()));
            repoSection.Add(new MenuElement("Search", GoToSearch, Octicon.Search.ToImage()));
            sections.Add(repoSection);
            
            if (ViewModel.PinnedRepositories.Any())
            {
                _favoriteRepoSection = new Section { HeaderView = new MenuSectionView("Favorite Repositories") };
                foreach (var pinnedRepository in ViewModel.PinnedRepositories)
                {
                    var element = new PinnedRepoElement(pinnedRepository);
                    element.Clicked.Subscribe(_ => GoToRepository(pinnedRepository.Owner, pinnedRepository.Name));
                    _favoriteRepoSection.Add(element);
                }

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
            gistsSection.Add(new MenuElement("My Gists", GoToOwnedGists, Octicon.Gist.ToImage()));
            gistsSection.Add(new MenuElement("Starred", GoToStarredGists, Octicon.Star.ToImage()));
            gistsSection.Add(new MenuElement("Public", GoToPublicGists, Octicon.Globe.ToImage()));
            sections.Add(gistsSection);

            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences") };
            sections.Add(infoSection);
            infoSection.Add(new MenuElement("Settings", GoToSettings, Octicon.Gear.ToImage()));

            if (ViewModel.ShouldShowUpgrades)
                infoSection.Add(new MenuElement("Upgrades", GoToUpgrades, Octicon.Lock.ToImage()));
            
            infoSection.Add(new MenuElement("Feedback & Support", GoToSupport, Octicon.CommentDiscussion.ToImage()));
            infoSection.Add(new MenuElement("Accounts", ProfileButtonClicked, Octicon.Person.ToImage()));

            Root.Reset(sections);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UpdateProfilePicture();
            CreateMenuRoot();

            // A user has been activated!
            var appService = Mvx.Resolve<IApplicationService>();
            appService.ActivationAction?.Invoke();
            appService.ActivationAction = null;

            #if DEBUG
            GC.Collect();
            GC.Collect();
            GC.Collect();
            #endif
        }

        private void GoToSearch()
        {
            var vc = new ExploreViewController();
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToSettings()
        {
            var vc = new SettingsViewController();
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToRepository(string owner, string name)
        {
            var vc = new Repositories.RepositoryViewController(owner, name);
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToSupport()
        {
            var vc = IsAccountEnterprise 
                ? (UIViewController)new EnterpriseSupportViewController() 
                : new SupportViewController();
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToOwnedRepositories()
        {
            var vc = Repositories.RepositoriesViewController.CreateMineViewController();
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToStarredRepositories()
        {
            var vc = Repositories.RepositoriesViewController.CreateStarredViewController();
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToTrendingRepositories()
        {
            var vc = new Repositories.TrendingRepositoriesViewController();
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToOwnedGists()
        {
            var username = ViewModel.Account.Username;
            var vc = Gists.GistsViewController.CreateUserGistsViewController(username);
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToStarredGists()
        {
            var vc = Gists.GistsViewController.CreateStarredGistsViewController();
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToPublicGists()
        {
            var vc = Gists.GistsViewController.CreatePublicGistsViewController();
            NavigationController?.PushViewController(vc, true);
        }

        private void GoToUpgrades()
        {
            var vc = new UpgradeViewController();
            NavigationController?.PushViewController(vc, true);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            UpdateProfilePicture();
        }

        private void ProfileButtonClicked()
        {
            var vc = new AccountsViewController();
            vc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            vc.NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => DismissViewController(true, null);
            PresentViewController(new ThemedNavigationController(vc), true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.SeparatorInset = UIEdgeInsets.Zero;
            TableView.SeparatorColor = UIColor.FromRGB(50, 50, 50);
            TableView.TableFooterView = new UIView(new CGRect(0, 0, View.Bounds.Width, 0));
            TableView.BackgroundColor = UIColor.FromRGB(34, 34, 34);
            TableView.ScrollsToTop = false;

            if (!string.IsNullOrEmpty(ViewModel.Account.AvatarUrl))
                _profileButton.Uri = new Uri(ViewModel.Account.AvatarUrl);

            ViewModel.Bind(x => x.Notifications).Subscribe(x =>
            {
                if (_notifications != null)
                {
                    _notifications.NotificationNumber = x;
                }
            });

            ViewModel.Bind(x => x.Organizations).Subscribe(x => CreateMenuRoot());

            ViewModel.LoadCommand.Execute(null);

        }

        private class PinnedRepoElement : MenuElement
        {
            public Core.Data.PinnedRepository PinnedRepo { get; private set; }

            public PinnedRepoElement(Core.Data.PinnedRepository pinnedRepo)
                : base(pinnedRepo.Name, null, Octicon.Repo.ToImage())
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
            private readonly WeakReference<MenuViewController> _parent;
            public EditSource(MenuViewController dvc) 
                : base (dvc)
            {
                _parent = new WeakReference<MenuViewController>(dvc);
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

