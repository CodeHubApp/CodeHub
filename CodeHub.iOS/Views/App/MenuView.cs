using System;
using CodeFramework.iOS.Elements;
using CodeFramework.iOS.ViewComponents;
using CodeHub.Core.ViewModels.App;
using MonoTouch.UIKit;
using System.Linq;
using ReactiveUI;
using Xamarin.Utilities.DialogElements;
using System.Collections.Generic;
using CodeHub.Core.Data;
using CodeHub.Core.Utilities;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.iOS.Views.App
{
	public class MenuView : MenuBaseViewController<MenuViewModel>
    {
        private MenuElement _notifications;
		private Section _favoriteRepoSection;

	    protected override void CreateMenuRoot()
		{
            var username = ViewModel.Account.Username;
            ProfileButton.Name = string.IsNullOrEmpty(ViewModel.Account.Name) ? ViewModel.Account.Username : ViewModel.Account.Name;
            ProfileButton.Username = ViewModel.Account.Email;
            ProfileButton.ImageUri = ViewModel.Account.AvatarUrl;

            var sections = new List<Section>();

            sections.Add(new Section
            {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.ExecuteIfCan(), Images.Person),
                (_notifications = new MenuElement("Notifications", () => ViewModel.GoToNotificationsCommand.ExecuteIfCan(), Images.Notifications) { NotificationNumber = ViewModel.Notifications }),
                new MenuElement("News", () => ViewModel.GoToNewsCommand.ExecuteIfCan(), Images.News),
                new MenuElement("Issues", () => ViewModel.GoToMyIssuesCommand.ExecuteIfCan(), Images.Flag)
            });

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(username, () => ViewModel.GoToMyEvents.ExecuteIfCan(), Images.Event) { ImageUri = new Uri(ViewModel.Account.AvatarUrl) });
            if (ViewModel.Organizations != null && ViewModel.Account.ShowOrganizationsInEvents)
            {
                eventsSection.Add(ViewModel.Organizations.Select(x =>
                    new MenuElement(x.Login, () => ViewModel.GoToOrganizationEventsCommand.Execute(x), Images.Event) { ImageUri = new Uri(x.AvatarUrl) }));
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
                    new MenuElement(x.Login, () => ViewModel.GoToOrganizationCommand.ExecuteIfCan(x), Images.Team) { ImageUri = new Uri(x.AvatarUrl) }));
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
            infoSection.Add(new MenuElement("Feedback & Support", PresentUserVoice, Images.Flag));
            infoSection.Add(new MenuElement("Accounts", () => ProfileButtonClicked(this, EventArgs.Empty), Images.User));

            Root.Reset(sections);
		}

        private void PresentUserVoice()
        {
//            var config = new UserVoice.UVConfig() {
//                Key = "95D8N9Q3UT1Asn89F7d3lA",
//                Secret = "xptp5xR6RtqTPpcopKrmOFWVQ4AIJEvr2LKx6KFGgE4",
//                Site = "codehub.uservoice.com",
//                ShowContactUs = true,
//                ShowForum = true,
//                ShowPostIdea = true,
//                ShowKnowledgeBase = true,
//            };
//            UserVoice.UserVoice.Initialize(config);
//            UserVoice.UserVoice.PresentUserVoiceInterfaceForParentViewController(this);
        }

        protected override void ProfileButtonClicked(object sender, EventArgs e)
        {
            ViewModel.GoToAccountsCommand.ExecuteIfCan();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			TableView.SeparatorInset = UIEdgeInsets.Zero;
			TableView.SeparatorColor = UIColor.FromRGB(50, 50, 50);

            ViewModel.WhenAnyValue(x => x.Notifications).Subscribe(x =>
            {
                if (_notifications == null)
                    return;
                _notifications.NotificationNumber = x;
                Root.Reload(_notifications);
            });

            ViewModel.WhenAnyValue(x => x.Organizations).Subscribe(x => CreateMenuRoot());

            ViewModel.LoadCommand.ExecuteIfCan();
        }

		private class PinnedRepoElement : MenuElement
		{
            public PinnedRepository PinnedRepo { get; private set; }

			public PinnedRepoElement(PinnedRepository pinnedRepo, System.Windows.Input.ICommand command)
				: base(pinnedRepo.Name, () => command.Execute(new RepositoryIdentifier { Owner = pinnedRepo.Owner, Name = pinnedRepo.Name }), Images.Repo)
			{
				PinnedRepo = pinnedRepo;

                // BUG FIX: App keeps getting relocated so the URLs become off
                if (PinnedRepo.ImageUri.EndsWith("repository.png", StringComparison.Ordinal))
                {
                    Image = UIImage.FromFile("Images/repository.png");
                }
                else if (PinnedRepo.ImageUri.EndsWith("repository_fork.png", StringComparison.Ordinal))
                {
                    Image = UIImage.FromFile("Images/repository_fork.png");
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

			if (_favoriteRepoSection.Count == 1)
			{
				Root.Remove(_favoriteRepoSection);
				_favoriteRepoSection = null;
			}
			else
			{
				_favoriteRepoSection.Remove(el);
			}
		}

		public override Source CreateSizingSource(bool unevenRows)
		{
			return new EditSource(this);
		}

		private class EditSource : Source
		{
			private readonly MenuView _parent;
			public EditSource(MenuView dvc) 
				: base (dvc)
			{
				_parent = dvc;
			}

			public override bool CanEditRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection == null)
					return false;
				if (_parent.Root[indexPath.Section] == _parent._favoriteRepoSection)
					return true;
				return false;
			}

			public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection != null && _parent.Root[indexPath.Section] == _parent._favoriteRepoSection)
					return UITableViewCellEditingStyle.Delete;
				return UITableViewCellEditingStyle.None;
			}

			public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				switch (editingStyle)
				{
					case UITableViewCellEditingStyle.Delete:
						var section = _parent.Root[indexPath.Section];
						var element = section[indexPath.Row];
						_parent.DeletePinnedRepo(element as PinnedRepoElement);
						break;
				}
			}
		}
    }
}

