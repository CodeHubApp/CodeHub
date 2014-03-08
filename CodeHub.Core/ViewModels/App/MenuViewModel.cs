using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Accounts;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.User;
using System.Linq;
using CodeFramework.Core.Utils;
using CodeFramework.Core.ViewModels.App;
using CodeHub.Core.Messages;
using Cirrious.MvvmCross.Plugins.Messenger;
using System;

namespace CodeHub.Core.ViewModels.App
{
	public class MenuViewModel : BaseMenuViewModel
    {
        private readonly IApplicationService _application;
		private int _notifications;
		private List<string> _organizations;
		private readonly MvxSubscriptionToken _notificationCountToken;

		public int Notifications
        {
            get { return _notifications; }
            set { _notifications = value; RaisePropertyChanged(() => Notifications); }
        }

		public List<string> Organizations
        {
            get { return _organizations; }
            set { _organizations = value; RaisePropertyChanged(() => Organizations); }
        }
		
        public GitHubAccount Account
        {
            get { return _application.Account; }
        }
		
        public MenuViewModel(IApplicationService application)
        {
            _application = application;
			_notificationCountToken = Messenger.SubscribeOnMainThread<NotificationCountMessage>(OnNotificationCountMessage);
        }

		private void OnNotificationCountMessage(NotificationCountMessage msg)
		{
			Notifications = msg.Count;
		}

        public ICommand GoToAccountsCommand
        {
            get { return new MvxCommand(() => this.ShowViewModel<AccountsViewModel>()); }
        }

		[PotentialStartupViewAttribute("Profile")]
        public ICommand GoToProfileCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = _application.Account.Username })); }
        }

		[PotentialStartupViewAttribute("Notifications")]
        public ICommand GoToNotificationsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NotificationsViewModel>(null)); }
        }

		[PotentialStartupViewAttribute("My Issues")]
        public ICommand GoToMyIssuesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<MyIssuesViewModel>(null)); }
        }

		[PotentialStartupViewAttribute("My Events")]
        public ICommand GoToMyEvents
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Account.Username })); }
        }

		[PotentialStartupViewAttribute("My Gists")]
        public ICommand GoToMyGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserGistsViewModel>(new UserGistsViewModel.NavObject { Username = Account.Username}));}
        }

		[PotentialStartupViewAttribute("Starred Gists")]
        public ICommand GoToStarredGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<StarredGistsViewModel>(null)); }
        }

		[PotentialStartupViewAttribute("Public Gists")]
        public ICommand GoToPublicGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<PublicGistsViewModel>(null)); }
        }

		[PotentialStartupViewAttribute("Starred Repositories")]
        public ICommand GoToStarredRepositoriesCommand
        {
			get { return new MvxCommand(() => ShowMenuViewModel<RepositoriesStarredViewModel>(null));}
        }

		[PotentialStartupViewAttribute("Owned Repositories")]
		public ICommand GoToOwnedRepositoriesCommand
		{
			get { return new MvxCommand(() => ShowMenuViewModel<UserRepositoriesViewModel>(new UserRepositoriesViewModel.NavObject { Username = Account.Username }));}
		}

		[PotentialStartupViewAttribute("Explore Repositories")]
		public ICommand GoToExploreRepositoriesCommand
		{
			get { return new MvxCommand(() => ShowMenuViewModel<RepositoriesExploreViewModel>(null));}
		}

		public ICommand GoToOrganizationEventsCommand
		{
			get { return new MvxCommand<string>(x => ShowMenuViewModel<Events.UserEventsViewModel>(new Events.UserEventsViewModel.NavObject { Username = x }));}
		}

		public ICommand GoToOrganizationCommand
		{
			get { return new MvxCommand<string>(x => ShowMenuViewModel<Organizations.OrganizationViewModel>(new Organizations.OrganizationViewModel.NavObject { Name = x }));}
		}

		[PotentialStartupViewAttribute("Organizations")]
		public ICommand GoToOrganizationsCommand
		{
			get { return new MvxCommand(() => ShowMenuViewModel<Organizations.OrganizationsViewModel>(new Organizations.OrganizationsViewModel.NavObject { Username = Account.Username }));}
		}

		[DefaultStartupViewAttribute]
		[PotentialStartupViewAttribute("News")]
        public ICommand GoToNewsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NewsViewModel>(null));}
        }

		public ICommand GoToSettingsCommand
		{
			get { return new MvxCommand(() => ShowMenuViewModel<SettingsViewModel>(null));}
		}

		public ICommand GoToRepositoryCommand
		{
			get { return new MvxCommand<RepositoryIdentifier>(x => ShowMenuViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner, Repository = x.Name }));}
		}

		public ICommand GoToAboutCommand
		{
			get { return new MvxCommand(() => ShowMenuViewModel<AboutViewModel>(null)); }
		}

        public ICommand GoToUpgradesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UpgradesViewModel>(null)); }
        }

        public ICommand LoadCommand
        {
            get { return new MvxCommand(Load);}    
        }

        private void Load()
        {
            var notificationRequest = this.GetApplication().Client.Notifications.GetAll();
            notificationRequest.RequestFromCache = false;
            notificationRequest.CheckIfModified = false;
            this.GetApplication().Client.ExecuteAsync(notificationRequest).ContinueWith(t =>
            {
                Notifications = t.Result.Data.Count;
            });

            this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.AuthenticatedUser.GetOrganizations()).ContinueWith(t =>
            {
                Organizations = t.Result.Data.Select(y => y.Login).ToList();

            });
        }
    }
}
