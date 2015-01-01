using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CodeHub.Core.Data;
using CodeHub.Core.Utilities;
using CodeHub.Core.ViewModels.App;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.Delegates;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.ViewControllers;
using CodeHub.iOS.Elements;
using CodeHub.iOS.ViewComponents;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.App
{
    public class MenuView : BaseTableViewController<MenuViewModel>
    {
        private readonly MenuElement _notifications;
		private Section _favoriteRepoSection;
        private DialogTableViewSource _dialogSource;
        private readonly MenuProfileView _profileButton;
        private readonly Section _gistsSection;
        private readonly Section _infoSection;
        private readonly Section _repoSection;
        private readonly Section _topSection;

        public MenuView()
        {
            _profileButton = new MenuProfileView(new RectangleF(0, 0, 320f, 44f));
            _profileButton.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            _profileButton.TouchUpInside += (sender, e) => ViewModel.GoToAccountsCommand.ExecuteIfCan();
            NavigationItem.TitleView = _profileButton;

            _topSection = new Section
            {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.ExecuteIfCan(), Images.Person),
                (_notifications = new MenuElement("Notifications", () => ViewModel.GoToNotificationsCommand.ExecuteIfCan(), Images.Inbox)),
                new MenuElement("News", () => ViewModel.GoToNewsCommand.ExecuteIfCan(), Images.RadioTower),
                new MenuElement("Issues", () => ViewModel.GoToMyIssuesCommand.ExecuteIfCan(), Images.IssueOpened)
            };

            _repoSection = new Section { HeaderView = new MenuSectionView("Repositories") };
            _repoSection.Add(new MenuElement("Owned", () => ViewModel.GoToOwnedRepositoriesCommand.ExecuteIfCan(), Images.Repo));
            _repoSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredRepositoriesCommand.ExecuteIfCan(), Images.Star));
            _repoSection.Add(new MenuElement("Watching", () => ViewModel.GoToWatchedRepositoriesCommand.ExecuteIfCan(), Images.Eye));
            _repoSection.Add(new MenuElement("Trending", () => ViewModel.GoToTrendingRepositoriesCommand.ExecuteIfCan(), Images.Pulse));
            _repoSection.Add(new MenuElement("Explore", () => ViewModel.GoToExploreRepositoriesCommand.ExecuteIfCan(), Images.Telescope));

            _gistsSection = new Section { HeaderView = new MenuSectionView("Gists") };
            _gistsSection.Add(new MenuElement("My Gists", () => ViewModel.GoToMyGistsCommand.ExecuteIfCan(), Images.Gist));
            _gistsSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredGistsCommand.ExecuteIfCan(), Images.Star));
            _gistsSection.Add(new MenuElement("Public", () => ViewModel.GoToPublicGistsCommand.ExecuteIfCan(), Images.Globe));

            _infoSection = new Section { HeaderView = new MenuSectionView("Info & Preferences") };
            _infoSection.Add(new MenuElement("Settings", () => ViewModel.GoToSettingsCommand.ExecuteIfCan(), Images.Gear));
            _infoSection.Add(new MenuElement("Upgrades", () => ViewModel.GoToUpgradesCommand.ExecuteIfCan(), Images.Lock));
            _infoSection.Add(new MenuElement("Feedback & Support", () => ViewModel.GoToFeedbackCommand.ExecuteIfCan(), Images.Question));
            _infoSection.Add(new MenuElement("Accounts", () => ViewModel.GoToAccountsCommand.ExecuteIfCan(), Images.Person));

            this.WhenAnyValue(x => x.ViewModel)
                .IsNotNull()
                .Subscribe(x => x.LoadCommand.ExecuteIfCan());

            this.WhenAnyValue(x => x.ViewModel.Account)
                .IsNotNull()
                .Subscribe(x =>
            {
                _profileButton.Name = string.IsNullOrEmpty(x.Name) ? x.Username : x.Name;
                _profileButton.Username = x.Email;
                _profileButton.ImageUri = x.AvatarUrl;
            });

            this.WhenAnyValue(x => x.ViewModel.Notifications).DistinctUntilChanged().Subscribe(x =>
            {
                _notifications.NotificationNumber = x;
                if (_dialogSource != null)
                    _dialogSource.Root.Reload(_notifications);
            });

            Appearing.Subscribe(_ => CreateMenuRoot());
            Appearing.Subscribe(_ =>
            {
                var frame = NavigationController.NavigationBar.Frame;
                _profileButton.Frame = new RectangleF(0, 0, frame.Width, frame.Height);
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Add some nice looking colors and effects
            TableView.TableFooterView = new UIView(new RectangleF(0, 0, View.Bounds.Width, 0));
            TableView.BackgroundColor = Themes.Theme.Current.MenuBackgroundColor;
            TableView.ScrollsToTop = false;
            TableView.SeparatorInset = UIEdgeInsets.Zero;
            TableView.SeparatorColor = Themes.Theme.Current.PrimaryNavigationBarColor;
            TableView.Source = _dialogSource = new MenuTableViewSource(this);

            ViewModel.WhenAnyValue(x => x.Organizations).Subscribe(x => CreateMenuRoot());
        }

	    private void CreateMenuRoot()
		{
            var sections = new List<Section>();
            sections.Add(_topSection);

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(ViewModel.Account.Username, () => ViewModel.GoToMyEvents.ExecuteIfCan(), Images.Rss, ViewModel.Account.AvatarUrl) { TintImage = false });
            if (ViewModel.Organizations != null && ViewModel.Account.ShowOrganizationsInEvents)
            {
                eventsSection.Add(ViewModel.Organizations.Select(x =>
                {
                    var avatarUri = new GitHubAvatar(x.AvatarUrl).ToUri(64).AbsoluteUri;
                    return new MenuElement(x.Login, () => ViewModel.GoToOrganizationEventsCommand.Execute(x), Images.Rss, avatarUri) { TintImage = false };
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
                    return new MenuElement(x.Login, () => ViewModel.GoToOrganizationCommand.ExecuteIfCan(x), Images.Organization, avatarUri) { TintImage = false };
                }));
            }
            else
                orgSection.Add(new MenuElement("Organizations", () => ViewModel.GoToOrganizationsCommand.ExecuteIfCan(), Images.Organization));

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
                return !pinnedRepository.ImageUri.StartsWith("http", StringComparison.Ordinal) 
                    ? null : new GitHubAvatar(pinnedRepository.ImageUri).ToUri(64).AbsoluteUri;
            }

			public PinnedRepoElement(PinnedRepository pinnedRepo, System.Windows.Input.ICommand command)
                : base(pinnedRepo.Name, 
                    () => command.Execute(new RepositoryIdentifier { Owner = pinnedRepo.Owner, Name = pinnedRepo.Name }), 
                    Images.Repo,
                    GetActualImage(pinnedRepo))
			{
				PinnedRepo = pinnedRepo;
                TintImage = false;
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
			private readonly MenuView _parent;

            public MenuTableViewSource(MenuView dvc) 
				: base (dvc.TableView)
			{
				_parent = dvc;
			}

			public override bool CanEditRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection == null)
					return false;
				if (Root[indexPath.Section] == _parent._favoriteRepoSection)
					return true;
				return false;
			}

			public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection != null && Root[indexPath.Section] == _parent._favoriteRepoSection)
					return UITableViewCellEditingStyle.Delete;
				return UITableViewCellEditingStyle.None;
			}

			public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, MonoTouch.Foundation.NSIndexPath indexPath)
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

