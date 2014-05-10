using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels;
using CodeHub.Core.ViewModels.App;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Linq;
using CodeFramework.Core.Utils;

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

	    protected override void CreateMenuRoot()
		{
            var username = ViewModel.Account.Username;
			Title = username;
            var root = new RootElement(username);

            root.Add(new Section
            {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.Execute(null), Images.Person),
                (_notifications = new MenuElement("Notifications", () => ViewModel.GoToNotificationsCommand.Execute(null), Images.Notifications) { NotificationNumber = ViewModel.Notifications }),
                new MenuElement("News", () => ViewModel.GoToNewsCommand.Execute(null), Images.News),
                new MenuElement("Issues", () => ViewModel.GoToMyIssuesCommand.Execute(null), Images.Flag)
            });

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(username, () => ViewModel.GoToMyEvents.Execute(null), Images.Event));
			if (ViewModel.Organizations != null && ViewModel.Account.ShowOrganizationsInEvents)
				ViewModel.Organizations.ForEach(x => eventsSection.Add(new MenuElement(x, () => ViewModel.GoToOrganizationEventsCommand.Execute(x), Images.Event)));
            root.Add(eventsSection);

            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
			repoSection.Add(new MenuElement("Owned", () => ViewModel.GoToOwnedRepositoriesCommand.Execute(null), Images.Repo));
			//repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
            repoSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredRepositoriesCommand.Execute(null), Images.Star));
            repoSection.Add(new MenuElement("Trending", () => ViewModel.GoToTrendingRepositoriesCommand.Execute(null), Images.Chart));
            repoSection.Add(new MenuElement("Explore", () => ViewModel.GoToExploreRepositoriesCommand.Execute(null), Images.Explore));
            root.Add(repoSection);
            
			if (ViewModel.PinnedRepositories.Count() > 0)
			{
				_favoriteRepoSection = new Section() { HeaderView = new MenuSectionView("Favorite Repositories".t()) };
				foreach (var pinnedRepository in ViewModel.PinnedRepositories)
					_favoriteRepoSection.Add(new PinnedRepoElement(pinnedRepository, ViewModel.GoToRepositoryCommand));
				root.Add(_favoriteRepoSection);
			}
			else
			{
				_favoriteRepoSection = null;
			}

            var orgSection = new Section() { HeaderView = new MenuSectionView("Organizations") };
			if (ViewModel.Organizations != null && ViewModel.Account.ExpandOrganizations)
				ViewModel.Organizations.ForEach(x => orgSection.Add(new MenuElement(x, () => ViewModel.GoToOrganizationCommand.Execute(x), Images.Team)));
            else
				orgSection.Add(new MenuElement("Organizations", () => ViewModel.GoToOrganizationsCommand.Execute(null), Images.Group));

            //There should be atleast 1 thing...
            if (orgSection.Elements.Count > 0)
                root.Add(orgSection);

            var gistsSection = new Section() { HeaderView = new MenuSectionView("Gists") };
            gistsSection.Add(new MenuElement("My Gists", () => ViewModel.GoToMyGistsCommand.Execute(null), Images.Script));
            gistsSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredGistsCommand.Execute(null), Images.Star2));
            gistsSection.Add(new MenuElement("Public", () => ViewModel.GoToPublicGistsCommand.Execute(null), Images.Public));
            root.Add(gistsSection);
//
            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences".t()) };
            root.Add(infoSection);
            infoSection.Add(new MenuElement("Settings".t(), () => ViewModel.GoToSettingsCommand.Execute(null), Images.Cog));
            infoSection.Add(new MenuElement("Upgrades".t(), () => ViewModel.GoToUpgradesCommand.Execute(null), Images.Unlocked));
			infoSection.Add(new MenuElement("About".t(), () => ViewModel.GoToAboutCommand.Execute(null), Images.Info));
            infoSection.Add(new MenuElement("Feedback & Support".t(), PresentUserVoice, Images.Flag));
            infoSection.Add(new MenuElement("Accounts".t(), () => ProfileButtonClicked(this, System.EventArgs.Empty), Images.User));
            Root = root;
		}

        private void PresentUserVoice()
        {
            var config = new UserVoice.UVConfig() {
                Key = "95D8N9Q3UT1Asn89F7d3lA",
                Secret = "xptp5xR6RtqTPpcopKrmOFWVQ4AIJEvr2LKx6KFGgE4",
                Site = "codehub.uservoice.com",
                ShowContactUs = true,
                ShowForum = true,
                ShowPostIdea = true,
                ShowKnowledgeBase = true,
            };
            UserVoice.UserVoice.Initialize(config);
            UserVoice.UserVoice.PresentUserVoiceInterfaceForParentViewController(this);
        }

        protected override void ProfileButtonClicked(object sender, System.EventArgs e)
        {
            ViewModel.GoToAccountsCommand.Execute(null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			TableView.SeparatorInset = UIEdgeInsets.Zero;
			TableView.SeparatorColor = UIColor.FromRGB(50, 50, 50);

			if (!string.IsNullOrEmpty(ViewModel.Account.AvatarUrl))
				ProfileButton.Uri = new System.Uri(ViewModel.Account.AvatarUrl);

            ViewModel.Bind(x => x.Notifications, x =>
            {
                if (_notifications != null)
                {
                    _notifications.NotificationNumber = x;
                    Root.Reload(_notifications, UITableViewRowAnimation.None);
                }
            });

            ViewModel.Bind(x => x.Organizations, x => CreateMenuRoot());

            ViewModel.LoadCommand.Execute(null);
        }

		private class PinnedRepoElement : MenuElement
		{
			public CodeFramework.Core.Data.PinnedRepository PinnedRepo
			{
				get;
				private set; 
			}

			public PinnedRepoElement(CodeFramework.Core.Data.PinnedRepository pinnedRepo, System.Windows.Input.ICommand command)
				: base(pinnedRepo.Name, () => command.Execute(new RepositoryIdentifier { Owner = pinnedRepo.Owner, Name = pinnedRepo.Name }), Images.Repo)
			{
				PinnedRepo = pinnedRepo;

                // BUG FIX: App keeps getting relocated so the URLs become off
                if (PinnedRepo.ImageUri.EndsWith("repository.png", System.StringComparison.Ordinal))
                {
                    Image = UIImage.FromFile("Images/repository.png");
                }
                else if (PinnedRepo.ImageUri.EndsWith("repository_fork.png", System.StringComparison.Ordinal))
                {
                    Image = UIImage.FromFile("Images/repository_fork.png");
                }
                else
                {
                    ImageUri = new System.Uri(PinnedRepo.ImageUri);
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

		public override DialogViewController.Source CreateSizingSource(bool unevenRows)
		{
			return new EditSource(this);
		}

		private class EditSource : SizingSource
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

