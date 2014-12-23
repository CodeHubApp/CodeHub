using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
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

namespace CodeHub.iOS.Views.App
{
    public class MenuView : ReactiveTableViewController<MenuViewModel>
    {
        private MenuElement _notifications;
		private Section _favoriteRepoSection;
        private DialogTableViewSource _dialogSource;
        private readonly MenuProfileView _profileButton;

        public MenuView()
        {
            _profileButton = new MenuProfileView(new RectangleF(0, 0, 320f, 44f));
            _profileButton.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            _profileButton.TouchUpInside += (sender, e) => ViewModel.GoToAccountsCommand.ExecuteIfCan();
            NavigationItem.TitleView = _profileButton;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            CreateMenuRoot();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Add some nice looking colors and effects
            TableView.TableFooterView = new UIView(new RectangleF(0, 0, View.Bounds.Width, 0));
            TableView.BackgroundColor = UIColor.FromRGB(34, 34, 34);
            TableView.ScrollsToTop = false;
            TableView.SeparatorInset = UIEdgeInsets.Zero;
            TableView.SeparatorColor = UIColor.FromRGB(50, 50, 50);
            TableView.Source = _dialogSource = new MenuTableViewSource(this);

            ViewModel.WhenAnyValue(x => x.Notifications).Where(_ => _notifications != null).Subscribe(x =>
            {
                _notifications.NotificationNumber = x;
                _dialogSource.Root.Reload(_notifications);
            });

            ViewModel.WhenAnyValue(x => x.Organizations).Subscribe(x => CreateMenuRoot());

            ViewModel.LoadCommand.ExecuteIfCan();
        }

	    private void CreateMenuRoot()
		{
            var username = ViewModel.Account.Username;
            _profileButton.Name = string.IsNullOrEmpty(ViewModel.Account.Name) ? ViewModel.Account.Username : ViewModel.Account.Name;
            _profileButton.Username = ViewModel.Account.Email;
            _profileButton.ImageUri = ViewModel.Account.AvatarUrl;

            var sections = new List<Section>();

            sections.Add(new Section
            {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.ExecuteIfCan(), Images.Person),
                (_notifications = new MenuElement("Notifications", () => ViewModel.GoToNotificationsCommand.ExecuteIfCan(), Images.Notifications) { NotificationNumber = ViewModel.Notifications }),
                new MenuElement("News", () => ViewModel.GoToNewsCommand.ExecuteIfCan(), Images.News),
                new MenuElement("Issues", () => ViewModel.GoToMyIssuesCommand.ExecuteIfCan(), Images.Flag)
            });

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(username, () => ViewModel.GoToMyEvents.ExecuteIfCan(), Images.Event, ViewModel.Account.AvatarUrl));
            if (ViewModel.Organizations != null && ViewModel.Account.ShowOrganizationsInEvents)
            {
                eventsSection.Add(ViewModel.Organizations.Select(x =>
                    new MenuElement(x.Login, () => ViewModel.GoToOrganizationEventsCommand.Execute(x), Images.Event, x.AvatarUrl)));
            }
            sections.Add(eventsSection);

            var repoSection = new Section { HeaderView = new MenuSectionView("Repositories") };
			repoSection.Add(new MenuElement("Owned", () => ViewModel.GoToOwnedRepositoriesCommand.ExecuteIfCan(), Images.Repo));
			//repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
            repoSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredRepositoriesCommand.ExecuteIfCan(), Images.Star));
            repoSection.Add(new MenuElement("Trending", () => ViewModel.GoToTrendingRepositoriesCommand.ExecuteIfCan(), Images.Chart));
            repoSection.Add(new MenuElement("Explore", () => ViewModel.GoToExploreRepositoriesCommand.ExecuteIfCan(), Images.Explore));
            sections.Add(repoSection);
            
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
                    new MenuElement(x.Login, () => ViewModel.GoToOrganizationCommand.ExecuteIfCan(x), Images.Team, x.AvatarUrl)));
            }
            else
				orgSection.Add(new MenuElement("Organizations", () => ViewModel.GoToOrganizationsCommand.ExecuteIfCan(), Images.Group));

            //There should be atleast 1 thing...
            if (orgSection.Count > 0)
                sections.Add(orgSection);

            var gistsSection = new Section { HeaderView = new MenuSectionView("Gists") };
            gistsSection.Add(new MenuElement("My Gists", () => ViewModel.GoToMyGistsCommand.ExecuteIfCan(), Images.Script));
            gistsSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredGistsCommand.ExecuteIfCan(), Images.Star2));
            gistsSection.Add(new MenuElement("Public", () => ViewModel.GoToPublicGistsCommand.ExecuteIfCan(), Images.Public));
            sections.Add(gistsSection);
//
            var infoSection = new Section { HeaderView = new MenuSectionView("Info & Preferences") };
            sections.Add(infoSection);
            infoSection.Add(new MenuElement("Settings", () => ViewModel.GoToSettingsCommand.ExecuteIfCan(), Images.Cog));
            infoSection.Add(new MenuElement("Upgrades", () => ViewModel.GoToUpgradesCommand.ExecuteIfCan(), Images.Unlocked));
            infoSection.Add(new MenuElement("Feedback & Support", () => ViewModel.GoToFeedbackCommand.ExecuteIfCan(), Images.Flag));
            infoSection.Add(new MenuElement("Accounts", () => ViewModel.GoToAccountsCommand.ExecuteIfCan(), Images.User));

            _dialogSource.Root.Reset(sections);
		}

		private class PinnedRepoElement : MenuElement
		{
            public PinnedRepository PinnedRepo { get; private set; }

            private static UIImage GetStaticImage(PinnedRepository pinnedRepository)
            {
                if (pinnedRepository.ImageUri.EndsWith("repository.png", StringComparison.Ordinal))
                    return UIImage.FromFile("Images/repository.png");
                if (pinnedRepository.ImageUri.EndsWith("repository_fork.png", StringComparison.Ordinal))
                    return UIImage.FromFile("Images/repository_fork.png");
                return Images.Repo;
            }

            private static string GetActualImage(PinnedRepository pinnedRepository)
            {
                if (pinnedRepository.ImageUri.StartsWith("http", StringComparison.Ordinal))
                    return pinnedRepository.ImageUri;
                return null;
            }

			public PinnedRepoElement(PinnedRepository pinnedRepo, System.Windows.Input.ICommand command)
                : base(pinnedRepo.Name, 
                    () => command.Execute(new RepositoryIdentifier { Owner = pinnedRepo.Owner, Name = pinnedRepo.Name }), 
                    GetStaticImage(pinnedRepo),
                    GetActualImage(pinnedRepo))
			{
				PinnedRepo = pinnedRepo;
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

