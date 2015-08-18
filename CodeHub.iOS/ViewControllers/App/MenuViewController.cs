using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using CodeHub.Core.Data;
using CodeHub.Core.Utilities;
using CodeHub.Core.ViewModels.App;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;
using CodeHub.iOS.ViewControllers.Accounts;

namespace CodeHub.iOS.ViewControllers.App
{
    public class MenuViewController : BaseTableViewController<MenuViewModel>
    {
        private readonly MenuElement _notifications;
		private Section _favoriteRepoSection;
        private DialogTableViewSource _dialogSource;
        private readonly MenuProfileView _profileButton;
        private readonly Section _gistsSection;
        private readonly Section _infoSection;
        private readonly Section _repoSection;
        private readonly Section _topSection;
        private readonly MenuElement _trendingElement;

        public MenuViewController()
        {
            _profileButton = new MenuProfileView(new CGRect(0, 0, 320f, 44f));
            _profileButton.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            _profileButton.TouchUpInside += (sender, e) => ViewModel.GoToAccountsCommand.ExecuteIfCan();
            NavigationItem.TitleView = _profileButton;

            _topSection = new Section
            {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.ExecuteIfCan(), Octicon.Person.ToImage()),
                (_notifications = new MenuElement("Notifications", () => ViewModel.GoToNotificationsCommand.ExecuteIfCan(), Octicon.Inbox.ToImage())),
                new MenuElement("News", () => ViewModel.GoToNewsCommand.ExecuteIfCan(), Octicon.RadioTower.ToImage()),
                new MenuElement("Issues", () => ViewModel.GoToMyIssuesCommand.ExecuteIfCan(), Octicon.IssueOpened.ToImage())
            };

            _trendingElement = new MenuElement("Trending", () => ViewModel.GoToTrendingRepositoriesCommand.ExecuteIfCan(), Octicon.Pulse.ToImage());
            this.WhenAnyValue(x => x.ViewModel.Account)
                .Select(x => x != null && x.IsEnterprise)
                .StartWith(true)
                .Subscribe(x => _trendingElement.Hidden = x);

            _repoSection = new Section { HeaderView = new MenuSectionView("Repositories") };
            _repoSection.Add(new MenuElement("Owned", () => ViewModel.GoToOwnedRepositoriesCommand.ExecuteIfCan(), Octicon.Repo.ToImage()));
            _repoSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredRepositoriesCommand.ExecuteIfCan(), Octicon.Star.ToImage()));
            _repoSection.Add(new MenuElement("Watching", () => ViewModel.GoToWatchedRepositoriesCommand.ExecuteIfCan(), Octicon.Eye.ToImage()));
            _repoSection.Add(_trendingElement);
            _repoSection.Add(new MenuElement("Explore", () => ViewModel.GoToExploreRepositoriesCommand.ExecuteIfCan(), Octicon.Telescope.ToImage()));

            _gistsSection = new Section { HeaderView = new MenuSectionView("Gists") };
            _gistsSection.Add(new MenuElement("My Gists", () => ViewModel.GoToMyGistsCommand.ExecuteIfCan(), Octicon.Gist.ToImage()));
            _gistsSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredGistsCommand.ExecuteIfCan(), Octicon.Star.ToImage()));
            _gistsSection.Add(new MenuElement("Public", () => ViewModel.GoToPublicGistsCommand.ExecuteIfCan(), Octicon.Globe.ToImage()));

            _infoSection = new Section { HeaderView = new MenuSectionView("Info & Preferences") };
            _infoSection.Add(new MenuElement("Settings", () => ViewModel.GoToSettingsCommand.ExecuteIfCan(), Octicon.Gear.ToImage()));
            //RegisterForNotifications_infoSection.Add(new MenuElement("Go Pro", () => ViewModel.GoToUpgradesCommand.ExecuteIfCan(), Octicon.Lock.ToImage()));
            _infoSection.Add(new MenuElement("Feedback & Support", () => ViewModel.GoToFeedbackCommand.ExecuteIfCan(), Octicon.Question.ToImage()));
            _infoSection.Add(new MenuElement("Accounts", () => ViewModel.GoToAccountsCommand.ExecuteIfCan(), Octicon.Person.ToImage()));

            this.WhenAnyValue(x => x.ViewModel)
                .IsNotNull()
                .Subscribe(x => x.LoadCommand.ExecuteIfCan());

            this.WhenAnyValue(x => x.ViewModel.Account)
                .IsNotNull()
                .Subscribe(x =>
            {
                _profileButton.Text = string.IsNullOrEmpty(x.Name) ? x.Username : x.Name;
                _profileButton.Subtitle = x.Email;
            });

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Subscribe(x => _profileButton.SetImage(x.ToUri(64), Images.UnknownUser));

            this.WhenAnyValue(x => x.ViewModel.Notifications)
                .Subscribe(x => _notifications.NotificationNumber = x);

            Appearing.Subscribe(_ => CreateMenuRoot());
            Appearing.Subscribe(_ =>
            {
                var frame = NavigationController.NavigationBar.Frame;
                _profileButton.Frame = new CGRect(0, 0, frame.Width, frame.Height);
            });

            Appeared
                .Take(1)
                .Delay(TimeSpan.FromMilliseconds(250), RxApp.MainThreadScheduler)
                .Subscribe(_ => ViewModel.ActivateCommand.ExecuteIfCan());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Add some nice looking colors and effects
            TableView.TableFooterView = new UIView(new CGRect(0, 0, View.Bounds.Width, 0));
            TableView.BackgroundColor = Theme.MenuBackgroundColor;
            TableView.ScrollsToTop = false;
            TableView.SeparatorColor = Theme.PrimaryNavigationBarColor;
            TableView.Source = _dialogSource = new MenuTableViewSource(this);
            TableView.RowHeight = 54f;

            ViewModel.WhenAnyValue(x => x.Organizations).Subscribe(x => CreateMenuRoot());
            ViewModel.WhenAnyValue(x => x.PinnedRepositories).Subscribe(x => CreateMenuRoot());
        }

        protected override void HandleNavigation(CodeHub.Core.ViewModels.IBaseViewModel viewModel, UIViewController view)
        {
            if (view is AccountsViewController)
            {
                var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
                var rootNav = (UINavigationController)appDelegate.Window.RootViewController;
                viewModel.RequestDismiss.Subscribe(_ => rootNav.DismissViewController(true, null));
                rootNav.PresentViewController(new ThemedNavigationController(view), true, null);
            }
            else
            {
                base.HandleNavigation(viewModel, view);
            }
        }

	    private void CreateMenuRoot()
		{
            var sections = new List<Section>();
            sections.Add(_topSection);

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(ViewModel.Account.Username, () => ViewModel.GoToMyEvents.ExecuteIfCan(), Octicon.Rss.ToImage(), ViewModel.Account.AvatarUrl) { TintImage = false });
            if (ViewModel.Organizations != null && ViewModel.Account.ShowOrganizationsInEvents)
            {
                eventsSection.Add(ViewModel.Organizations.Select(x =>
                {
                    var avatarUri = new GitHubAvatar(x.AvatarUrl).ToUri(64).AbsoluteUri;
                    return new MenuElement(x.Login, () => ViewModel.GoToOrganizationEventsCommand.Execute(x), Octicon.Rss.ToImage(), avatarUri) { TintImage = false };
                }));
            }
            sections.Add(eventsSection);
            sections.Add(_repoSection);
            
			if (ViewModel.PinnedRepositories.Any())
			{
				_favoriteRepoSection = new Section { HeaderView = new MenuSectionView("Favorite Repositories") };
				foreach (var pinnedRepository in ViewModel.PinnedRepositories)
					_favoriteRepoSection.Add(new PinnedRepoElement(pinnedRepository, ViewModel.GoToRepositoryCommand));
                sections.Add(_favoriteRepoSection);
			}
			else
			{
				_favoriteRepoSection = null;
			}

            var orgSection = new Section { HeaderView = new MenuSectionView("Organizations") };
            if (ViewModel.Organizations != null && ViewModel.Account.ExpandOrganizations)
            {
                orgSection.Add(ViewModel.Organizations.Select(x =>
                {
                    var avatarUri = new GitHubAvatar(x.AvatarUrl).ToUri(64).AbsoluteUri;
                    return new MenuElement(x.Login, () => ViewModel.GoToOrganizationCommand.ExecuteIfCan(x), Octicon.Organization.ToImage(), avatarUri) { TintImage = false };
                }));
            }
            else
                orgSection.Add(new MenuElement("Organizations", () => ViewModel.GoToOrganizationsCommand.ExecuteIfCan(), Octicon.Organization.ToImage()));

            //There should be atleast 1 thing...
            if (orgSection.Count > 0)
                sections.Add(orgSection);

            sections.Add(_gistsSection);
            sections.Add(_infoSection);
            _dialogSource.Root.Reset(sections);
		}

		private class PinnedRepoElement : MenuElement
		{
            public PinnedRepository PinnedRepo { get; private set; }

            private static string GetActualImage(PinnedRepository pinnedRepository)
            {
                if (pinnedRepository.ImageUri == null)
                    return null;
                return !pinnedRepository.ImageUri.StartsWith("http", StringComparison.Ordinal) 
                    ? null : new GitHubAvatar(pinnedRepository.ImageUri).ToUri(64).AbsoluteUri;
            }

			public PinnedRepoElement(PinnedRepository pinnedRepo, System.Windows.Input.ICommand command)
                : base(pinnedRepo.Name, 
                    () => command.Execute(new RepositoryIdentifier(pinnedRepo.Owner, pinnedRepo.Name)), 
                    Octicon.Repo.ToImage(),
                    GetActualImage(pinnedRepo))
			{
				PinnedRepo = pinnedRepo;
                TintImage = pinnedRepo.ImageUri == null;
			}
		}

		private void DeletePinnedRepo(PinnedRepoElement el)
		{
			ViewModel.DeletePinnedRepositoryCommand.Execute(el.PinnedRepo);

			if (_favoriteRepoSection.Count == 1)
			{
                _dialogSource.Root.Remove(_favoriteRepoSection);
				_favoriteRepoSection = null;
			}
			else
			{
                _favoriteRepoSection.Remove(el);
			}
		}

        private class MenuTableViewSource : DialogTableViewSource
		{
			private readonly MenuViewController _parent;

            public MenuTableViewSource(MenuViewController dvc) 
				: base (dvc.TableView)
			{
				_parent = dvc;
			}

			public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection == null)
					return false;
				if (Root[indexPath.Section] == _parent._favoriteRepoSection)
					return true;
				return false;
			}

			public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection != null && Root[indexPath.Section] == _parent._favoriteRepoSection)
					return UITableViewCellEditingStyle.Delete;
				return UITableViewCellEditingStyle.None;
			}

			public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
			{
				switch (editingStyle)
				{
					case UITableViewCellEditingStyle.Delete:
						var section = Root[indexPath.Section];
						var element = section[indexPath.Row];
						_parent.DeletePinnedRepo(element as PinnedRepoElement);
						break;
				}
			}
		}
    }
}

