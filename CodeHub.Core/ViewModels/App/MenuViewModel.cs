using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.Utilities;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.Notifications;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Settings;
using CodeHub.Core.ViewModels.Users;
using System.Threading;
using System.Reactive.Threading.Tasks;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseMenuViewModel
    {
        private readonly IApplicationService _applicationService;
		private int _notifications;

		public int Notifications
        {
            get { return _notifications; }
            set { this.RaiseAndSetIfChanged(ref _notifications, value); }
        }

        private IReadOnlyList<Octokit.Organization> _organizations;
        public IReadOnlyList<Octokit.Organization> Organizations
        {
            get { return _organizations; }
            set { this.RaiseAndSetIfChanged(ref _organizations, value); }
        }
		
        public GitHubAccount Account
        {
            get { return _applicationService.Account; }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }
		
        public MenuViewModel(IApplicationService applicationService, IAccountsService accountsService)
            : base(accountsService)
        {
            _applicationService = applicationService;

            GoToNotificationsCommand = ReactiveCommand.Create();
            GoToNotificationsCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<NotificationsViewModel>();
                NavigateTo(vm);
            });

            GoToAccountsCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<AccountsViewModel>()));

            GoToProfileCommand = ReactiveCommand.Create();
            GoToProfileCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<UserViewModel>();
                vm.Username = Account.Username;
                NavigateTo(vm);
            });

            GoToMyIssuesCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<MyIssuesViewModel>()));

            GoToUpgradesCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<UpgradesViewModel>()));
     
            GoToRepositoryCommand = ReactiveCommand.Create();
            GoToRepositoryCommand.OfType<RepositoryIdentifier>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner;
                vm.RepositoryName = x.Name;
                NavigateTo(vm);
            });

            GoToSettingsCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<SettingsViewModel>()));

            GoToNewsCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<NewsViewModel>()));

            GoToOrganizationsCommand = ReactiveCommand.Create();
            GoToOrganizationsCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<OrganizationsViewModel>();
                vm.Username = Account.Username;
                NavigateTo(vm);
            });

            GoToTrendingRepositoriesCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<RepositoriesTrendingViewModel>()));

            GoToExploreRepositoriesCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<RepositoriesExploreViewModel>()));

            GoToOrganizationEventsCommand = ReactiveCommand.Create();
            GoToOrganizationEventsCommand.OfType<Octokit.Organization>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<UserEventsViewModel>();
                vm.Username = x.Login;
                NavigateTo(vm);
            });

            GoToOrganizationCommand = ReactiveCommand.Create();
            GoToOrganizationCommand.OfType<Octokit.Organization>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<OrganizationViewModel>();
                vm.Username = x.Login;
                NavigateTo(vm);
            });

            GoToOwnedRepositoriesCommand = ReactiveCommand.Create();
            GoToOwnedRepositoriesCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<UserRepositoriesViewModel>();
                vm.Username = Account.Username;
                NavigateTo(vm);
            });

            GoToStarredRepositoriesCommand = ReactiveCommand.Create().WithSubscription(
                _ => NavigateTo(this.CreateViewModel<RepositoriesStarredViewModel>()));

            GoToWatchedRepositoriesCommand = ReactiveCommand.Create().WithSubscription(
                _ => NavigateTo(this.CreateViewModel<RepositoriesWatchedViewModel>()));

            GoToPublicGistsCommand = ReactiveCommand.Create().WithSubscription(
                _ => NavigateTo(this.CreateViewModel<PublicGistsViewModel>()));

            GoToStarredGistsCommand = ReactiveCommand.Create().WithSubscription(
                _ => NavigateTo(this.CreateViewModel<StarredGistsViewModel>()));

            GoToMyGistsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserGistsViewModel>();
                vm.Username = Account.Username;
                NavigateTo(vm);
            });

            GoToMyEvents = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserEventsViewModel>();
                vm.Username = Account.Username;
                NavigateTo(vm);
            });

            GoToFeedbackCommand = ReactiveCommand.Create().WithSubscription(_ => 
                NavigateTo(this.CreateViewModel<SupportViewModel>()));

            LoadCommand = ReactiveCommand.CreateAsyncTask(_ =>
            {
                var notifications = Observable.FromAsync(applicationService.GitHubClient.Notification.GetAllForCurrent);
                notifications.ObserveOn(SynchronizationContext.Current).Subscribe(x => Notifications = x.Count);

                var organizations = Observable.FromAsync(applicationService.GitHubClient.Organization.GetAllForCurrent);
                organizations.ObserveOn(SynchronizationContext.Current).Subscribe(x => Organizations = x);

                return notifications.Select(x => Unit.Default).Merge(organizations.Select(x => Unit.Default)).ToTask();
            });
        }

        public IReactiveCommand<object> GoToAccountsCommand { get; private set; }

		[PotentialStartupViewAttribute("Profile")]
        public IReactiveCommand<object> GoToProfileCommand { get; private set; }

		[PotentialStartupViewAttribute("Notifications")]
        public IReactiveCommand<object> GoToNotificationsCommand { get; private set; }

		[PotentialStartupViewAttribute("My Issues")]
        public IReactiveCommand<object> GoToMyIssuesCommand { get; private set; }

		[PotentialStartupViewAttribute("My Events")]
        public IReactiveCommand GoToMyEvents { get; private set; }

		[PotentialStartupViewAttribute("My Gists")]
        public IReactiveCommand<object> GoToMyGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Starred Gists")]
        public IReactiveCommand<object> GoToStarredGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Public Gists")]
        public IReactiveCommand<object> GoToPublicGistsCommand { get; private set; }

		[PotentialStartupViewAttribute("Starred Repositories")]
        public IReactiveCommand<object> GoToStarredRepositoriesCommand { get; private set; }

        [PotentialStartupViewAttribute("Watched Repositories")]
        public IReactiveCommand<object> GoToWatchedRepositoriesCommand { get; private set; }

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
		[PotentialStartupView("News")]
        public IReactiveCommand<object> GoToNewsCommand { get; private set; }

		public IReactiveCommand<object> GoToSettingsCommand { get; private set; }

		public IReactiveCommand<object> GoToRepositoryCommand { get; private set; }

        public IReactiveCommand<object> GoToUpgradesCommand { get; private set; }

        public IReactiveCommand GoToFeedbackCommand { get; private set; }
    }
}
