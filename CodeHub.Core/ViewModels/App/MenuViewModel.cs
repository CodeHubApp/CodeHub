using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using CodeFramework.Core.Services;
using CodeFramework.Core.ViewModels.Application;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using CodeFramework.Core.Utils;
using CodeHub.Core.ViewModels.Notifications;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseMenuViewModel, IMainViewModel
    {
        private readonly IApplicationService _applicationService;
		private int _notifications;
		private List<string> _organizations;

		public int Notifications
        {
            get { return _notifications; }
            set { this.RaiseAndSetIfChanged(ref _notifications, value); }
        }

		public List<string> Organizations
        {
            get { return _organizations; }
            set { this.RaiseAndSetIfChanged(ref _organizations, value); }
        }
		
        public GitHubAccount Account
        {
            get { return _applicationService.Account; }
        }

        public IReactiveCommand LoadCommand { get; private set; }
		
        public MenuViewModel(IApplicationService applicationService, IAccountsService accountsService)
            : base(accountsService)
        {
            _applicationService = applicationService;

            GoToNotificationsCommand = new ReactiveCommand();
            GoToNotificationsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<NotificationsViewModel>();
                ShowViewModel(vm);
            });

            GoToAccountsCommand = new ReactiveCommand();
            GoToAccountsCommand.Subscribe(_ => CreateAndShowViewModel<AccountsViewModel>());

            GoToProfileCommand = new ReactiveCommand();
            GoToProfileCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            GoToMyIssuesCommand = new ReactiveCommand();
            GoToMyIssuesCommand.Subscribe(_ => CreateAndShowViewModel<MyIssuesViewModel>());

            GoToUpgradesCommand = new ReactiveCommand();
            GoToUpgradesCommand.Subscribe(_ => CreateAndShowViewModel<UpgradesViewModel>());

            GoToAboutCommand = new ReactiveCommand();
            GoToAboutCommand.Subscribe(_ => CreateAndShowViewModel<AboutViewModel>());

            GoToRepositoryCommand = new ReactiveCommand();
            GoToRepositoryCommand.OfType<RepositoryIdentifier>().Subscribe(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner;
                vm.RepositoryName = x.Name;
                ShowViewModel(vm);
            });

            GoToSettingsCommand = new ReactiveCommand();
            GoToSettingsCommand.Subscribe(_ => CreateAndShowViewModel<SettingsViewModel>());

            GoToNewsCommand = new ReactiveCommand();
            GoToNewsCommand.Subscribe(_ => CreateAndShowViewModel<NewsViewModel>());

            GoToOrganizationsCommand = new ReactiveCommand();
            GoToOrganizationsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<OrganizationsViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            GoToTrendingRepositoriesCommand = new ReactiveCommand();
            GoToTrendingRepositoriesCommand.Subscribe(_ => CreateAndShowViewModel<RepositoriesTrendingViewModel>());

            GoToExploreRepositoriesCommand = new ReactiveCommand();
            GoToExploreRepositoriesCommand.Subscribe(_ => CreateAndShowViewModel<RepositoriesExploreViewModel>());

            GoToOrganizationEventsCommand = new ReactiveCommand();
            GoToOrganizationEventsCommand.OfType<string>().Subscribe(name =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = name;
                ShowViewModel(vm);
            });

            GoToOrganizationCommand = new ReactiveCommand();
            GoToOrganizationCommand.OfType<string>().Subscribe(name =>
            {
                var vm = CreateViewModel<OrganizationViewModel>();
                vm.Name = name;
                ShowViewModel(vm);
            });

            GoToOwnedRepositoriesCommand = new ReactiveCommand();
            GoToOwnedRepositoriesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserRepositoriesViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            GoToStarredRepositoriesCommand = new ReactiveCommand();
            GoToStarredRepositoriesCommand.Subscribe(_ => CreateAndShowViewModel<RepositoriesStarredViewModel>());

            GoToPublicGistsCommand = new ReactiveCommand();
            GoToPublicGistsCommand.Subscribe(_ => CreateAndShowViewModel<PublicGistsViewModel>());

            GoToStarredGistsCommand = new ReactiveCommand();
            GoToStarredGistsCommand.Subscribe(_ => CreateAndShowViewModel<StarredGistsViewModel>());

            GoToMyGistsCommand = new ReactiveCommand();
            GoToMyGistsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserGistsViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            GoToMyEvents = new ReactiveCommand();
            GoToMyEvents.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            LoadCommand = new ReactiveCommand();
            LoadCommand.Subscribe(_ =>
            {
                var notificationRequest = applicationService.Client.Notifications.GetAll();
                notificationRequest.RequestFromCache = false;
                notificationRequest.CheckIfModified = false;

                applicationService.Client.ExecuteAsync(notificationRequest)
                    .ContinueWith(t => Notifications = t.Result.Data.Count);

                applicationService.Client.ExecuteAsync(applicationService.Client.AuthenticatedUser.GetOrganizations())
                    .ContinueWith(t => Organizations = t.Result.Data.Select(y => y.Login).ToList());
            });

        }

        public IReactiveCommand GoToAccountsCommand { get; private set; }

		[PotentialStartupViewAttribute("Profile")]
        public IReactiveCommand GoToProfileCommand { get; private set; }

		[PotentialStartupViewAttribute("Notifications")]
        public IReactiveCommand GoToNotificationsCommand { get; private set; }

		[PotentialStartupViewAttribute("My Issues")]
        public IReactiveCommand GoToMyIssuesCommand { get; private set; }

		[PotentialStartupViewAttribute("My Events")]
        public IReactiveCommand GoToMyEvents { get; private set; }

		[PotentialStartupViewAttribute("My Gists")]
        public IReactiveCommand GoToMyGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Starred Gists")]
        public IReactiveCommand GoToStarredGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Public Gists")]
        public IReactiveCommand GoToPublicGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Starred Repositories")]
        public IReactiveCommand GoToStarredRepositoriesCommand { get; private set; }

		[PotentialStartupViewAttribute("Owned Repositories")]
		public IReactiveCommand GoToOwnedRepositoriesCommand { get; private set; }

		[PotentialStartupViewAttribute("Explore Repositories")]
		public IReactiveCommand GoToExploreRepositoriesCommand { get; private set; }

        [PotentialStartupViewAttribute("Trending Repositories")]
        public IReactiveCommand GoToTrendingRepositoriesCommand { get; private set; }

		public IReactiveCommand GoToOrganizationEventsCommand { get; private set; }

		public IReactiveCommand GoToOrganizationCommand { get; private set; }

		[PotentialStartupViewAttribute("Organizations")]
		public IReactiveCommand GoToOrganizationsCommand { get; private set; }

		[DefaultStartupViewAttribute]
		[PotentialStartupViewAttribute("News")]
        public IReactiveCommand GoToNewsCommand { get; private set; }

		public IReactiveCommand GoToSettingsCommand { get; private set; }

		public IReactiveCommand GoToRepositoryCommand { get; private set; }

		public IReactiveCommand GoToAboutCommand { get; private set; }

        public IReactiveCommand GoToUpgradesCommand { get; private set; }
    }
}
