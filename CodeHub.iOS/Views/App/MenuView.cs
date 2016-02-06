using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.App;
using MonoTouch.Dialog;
using UIKit;
using System.Linq;
using CodeFramework.Core.Utils;
using Cirrious.CrossCore;
using CodeHub.Core.Services;
using System;

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
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.Execute(null), Octicon.Person.ToImage()),
                (_notifications = new MenuElement("Notifications", () => ViewModel.GoToNotificationsCommand.Execute(null), Octicon.Inbox.ToImage()) { NotificationNumber = ViewModel.Notifications }),
                new MenuElement("News", () => ViewModel.GoToNewsCommand.Execute(null), Octicon.RadioTower.ToImage()),
                new MenuElement("Issues", () => ViewModel.GoToMyIssuesCommand.Execute(null), Octicon.IssueOpened.ToImage())
            });

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(username, () => ViewModel.GoToMyEvents.Execute(null), Octicon.Rss.ToImage()));
            if (ViewModel.Organizations != null && ViewModel.Account.ShowOrganizationsInEvents)
            {
                foreach (var org in ViewModel.Organizations)
                {
                    Uri avatarUri;
                    Uri.TryCreate(org.AvatarUrl, UriKind.Absolute, out avatarUri);
                    eventsSection.Add(new MenuElement(org.Login, () => ViewModel.GoToOrganizationEventsCommand.Execute(org.Login), Octicon.Rss.ToImage(), avatarUri));
                }
            }
            root.Add(eventsSection);

            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
            repoSection.Add(new MenuElement("Owned", () => ViewModel.GoToOwnedRepositoriesCommand.Execute(null), Octicon.Repo.ToImage()));
			//repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
            repoSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredRepositoriesCommand.Execute(null), Octicon.Star.ToImage()));
            repoSection.Add(new MenuElement("Trending", () => ViewModel.GoToTrendingRepositoriesCommand.Execute(null), Octicon.Pulse.ToImage()));
            repoSection.Add(new MenuElement("Explore", () => ViewModel.GoToExploreRepositoriesCommand.Execute(null), Octicon.Globe.ToImage()));
            root.Add(repoSection);
            
			if (ViewModel.PinnedRepositories.Count() > 0)
			{
				_favoriteRepoSection = new Section() { HeaderView = new MenuSectionView("Favorite Repositories") };
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
            {
                foreach (var org in ViewModel.Organizations)
                {
                    Uri avatarUri;
                    Uri.TryCreate(org.AvatarUrl, UriKind.Absolute, out avatarUri);
                    orgSection.Add(new MenuElement(org.Login, () => ViewModel.GoToOrganizationCommand.Execute(org.Login), Images.Avatar, avatarUri));
                }
            }
            else
                orgSection.Add(new MenuElement("Organizations", () => ViewModel.GoToOrganizationsCommand.Execute(null), Octicon.Organization.ToImage()));

            //There should be atleast 1 thing...
            if (orgSection.Elements.Count > 0)
                root.Add(orgSection);

            var gistsSection = new Section() { HeaderView = new MenuSectionView("Gists") };
            gistsSection.Add(new MenuElement("My Gists", () => ViewModel.GoToMyGistsCommand.Execute(null), Octicon.Gist.ToImage()));
            gistsSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredGistsCommand.Execute(null), Octicon.Star.ToImage()));
            gistsSection.Add(new MenuElement("Public", () => ViewModel.GoToPublicGistsCommand.Execute(null), Octicon.Globe.ToImage()));
            root.Add(gistsSection);
//
            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences") };
            root.Add(infoSection);
            infoSection.Add(new MenuElement("Settings", () => ViewModel.GoToSettingsCommand.Execute(null), Octicon.Gear.ToImage()));
            infoSection.Add(new MenuElement("Upgrades", () => ViewModel.GoToUpgradesCommand.Execute(null), Octicon.Lock.ToImage()));
            infoSection.Add(new MenuElement("Feedback & Support", PresentUserVoice, Octicon.CommentDiscussion.ToImage()));
            infoSection.Add(new MenuElement("Accounts", () => ProfileButtonClicked(this, System.EventArgs.Empty), Octicon.Person.ToImage()));
            Root = root;
		}

        private void PresentUserVoice()
        {
            ViewModel.GoToSupport.Execute(null);
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
			public CodeFramework.Core.Data.PinnedRepository PinnedRepo
			{
				get;
				private set; 
			}

			public PinnedRepoElement(CodeFramework.Core.Data.PinnedRepository pinnedRepo, System.Windows.Input.ICommand command)
                : base(pinnedRepo.Name, () => command.Execute(new RepositoryIdentifier { Owner = pinnedRepo.Owner, Name = pinnedRepo.Name }), Octicon.Repo.ToImage())
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

			public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection == null)
					return false;
				if (_parent.Root[indexPath.Section] == _parent._favoriteRepoSection)
					return true;
				return false;
			}

			public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection != null && _parent.Root[indexPath.Section] == _parent._favoriteRepoSection)
					return UITableViewCellEditingStyle.Delete;
				return UITableViewCellEditingStyle.None;
			}

			public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
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

