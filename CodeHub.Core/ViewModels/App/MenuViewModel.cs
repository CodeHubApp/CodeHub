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
using System.Threading.Tasks;

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

            GoToNotificationsCommand = ReactiveCommand.Create();
            GoToNotificationsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<NotificationsViewModel>();
                ShowViewModel(vm);
            });

            GoToAccountsCommand = ReactiveCommand.Create();
            GoToAccountsCommand.Subscribe(_ => CreateAndShowViewModel<AccountsViewModel>());

            GoToProfileCommand = ReactiveCommand.Create();
            GoToProfileCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            GoToMyIssuesCommand = ReactiveCommand.Create();
            GoToMyIssuesCommand.Subscribe(_ => CreateAndShowViewModel<MyIssuesViewModel>());

            GoToUpgradesCommand = ReactiveCommand.Create();
            GoToUpgradesCommand.Subscribe(_ => CreateAndShowViewModel<UpgradesViewModel>());

            GoToAboutCommand = ReactiveCommand.Create();
            GoToAboutCommand.Subscribe(_ => CreateAndShowViewModel<AboutViewModel>());

            GoToRepositoryCommand = ReactiveCommand.Create();
            GoToRepositoryCommand.OfType<RepositoryIdentifier>().Subscribe(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner;
                vm.RepositoryName = x.Name;
                ShowViewModel(vm);
            });

            GoToSettingsCommand = ReactiveCommand.Create();
            GoToSettingsCommand.Subscribe(_ => CreateAndShowViewModel<SettingsViewModel>());

            GoToNewsCommand = ReactiveCommand.Create();
            GoToNewsCommand.Subscribe(_ => CreateAndShowViewModel<NewsViewModel>());

            GoToOrganizationsCommand = ReactiveCommand.Create();
            GoToOrganizationsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<OrganizationsViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            GoToTrendingRepositoriesCommand = ReactiveCommand.Create();
            GoToTrendingRepositoriesCommand.Subscribe(_ => CreateAndShowViewModel<RepositoriesTrendingViewModel>());

            GoToExploreRepositoriesCommand = ReactiveCommand.Create();
            GoToExploreRepositoriesCommand.Subscribe(_ => CreateAndShowViewModel<RepositoriesExploreViewModel>());

            GoToOrganizationEventsCommand = ReactiveCommand.Create();
            GoToOrganizationEventsCommand.OfType<string>().Subscribe(name =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = name;
                ShowViewModel(vm);
            });

            GoToOrganizationCommand = ReactiveCommand.Create();
            GoToOrganizationCommand.OfType<string>().Subscribe(name =>
            {
                var vm = CreateViewModel<OrganizationViewModel>();
                vm.Name = name;
                ShowViewModel(vm);
            });

            GoToOwnedRepositoriesCommand = ReactiveCommand.Create();
            GoToOwnedRepositoriesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserRepositoriesViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            GoToStarredRepositoriesCommand = ReactiveCommand.Create();
            GoToStarredRepositoriesCommand.Subscribe(_ => CreateAndShowViewModel<RepositoriesStarredViewModel>());

            GoToPublicGistsCommand = ReactiveCommand.Create();
            GoToPublicGistsCommand.Subscribe(_ => CreateAndShowViewModel<PublicGistsViewModel>());

            GoToStarredGistsCommand = ReactiveCommand.Create();
            GoToStarredGistsCommand.Subscribe(_ => CreateAndShowViewModel<StarredGistsViewModel>());

            GoToMyGistsCommand = ReactiveCommand.Create();
            GoToMyGistsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserGistsViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            GoToMyEvents = ReactiveCommand.Create();
            GoToMyEvents.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = Account.Username;
                ShowViewModel(vm);
            });

            var loadCommand = ReactiveCommand.Create();
            loadCommand.Subscribe(_ =>
            {
                var notificationRequest = applicationService.Client.Notifications.GetAll();
                notificationRequest.RequestFromCache = false;
                notificationRequest.CheckIfModified = false;

                applicationService.Client.ExecuteAsync(notificationRequest)
                    .ContinueWith(t => Notifications = t.Result.Data.Count, TaskScheduler.FromCurrentSynchronizationContext());

                applicationService.Client.ExecuteAsync(applicationService.Client.AuthenticatedUser.GetOrganizations())
                    .ContinueWith(t => Organizations = t.Result.Data.Select(y => y.Login).ToList(), TaskScheduler.FromCurrentSynchronizationContext());
            });
            LoadCommand = loadCommand;

        }

        public IReactiveCommand<object> GoToAccountsCommand { get; private set; }

		[PotentialStartupViewAttribute("Profile")]
        public IReactiveCommand<object> GoToProfileCommand { get; private set; }

		[PotentialStartupViewAttribute("Notifications")]
        public IReactiveCommand<object> GoToNotificationsCommand { get; private set; }

		[PotentialStartupViewAttribute("My Issues")]
        public IReactiveCommand<object> GoToMyIssuesCommand { get; private set; }

		[PotentialStartupViewAttribute("My Events")]
        public IReactiveCommand<object> GoToMyEvents { get; private set; }

		[PotentialStartupViewAttribute("My Gists")]
        public IReactiveCommand<object> GoToMyGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Starred Gists")]
        public IReactiveCommand<object> GoToStarredGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Public Gists")]
        public IReactiveCommand<object> GoToPublicGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Starred Repositories")]
        public IReactiveCommand<object> GoToStarredRepositoriesCommand { get; private set; }

		[PotentialStartupViewAttribute("Owned Repositories")]
		public IReactiveCommand<object> GoToOwnedRepositoriesCommand { get; private set; }

		[PotentialStartupViewAttribute("Explore Repositories")]
		public IReactiveCommand<object> GoToExploreRepositoriesCommand { get; private set; }

        [PotentialStartupViewAttribute("Trending Repositories")]
        public IReactiveCommand<object> GoToTrendingRepositoriesCommand { get; private set; }

		public IReactiveCommand<object> GoToOrganizationEventsCommand { get; private set; }

		public IReactiveCommand<object> GoToOrganizationCommand { get; private set; }

		[PotentialStartupViewAttribute("Organizations")]
		public IReactiveCommand<object> GoToOrganizationsCommand { get; private set; }

		[DefaultStartupViewAttribute]
		[PotentialStartupViewAttribute("News")]
        public IReactiveCommand<object> GoToNewsCommand { get; private set; }

		public IReactiveCommand<object> GoToSettingsCommand { get; private set; }

		public IReactiveCommand<object> GoToRepositoryCommand { get; private set; }

		public IReactiveCommand<object> GoToAboutCommand { get; private set; }

        public IReactiveCommand<object> GoToUpgradesCommand { get; private set; }
    }
}
