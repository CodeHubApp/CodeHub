using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.Utilities;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.Activity;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Settings;
using CodeHub.Core.ViewModels.Users;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly ISessionService _applicationService;
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

        public IReactiveCommand<object> DeletePinnedRepositoryCommand { get; private set; }

        public IReadOnlyList<PinnedRepository> PinnedRepositories 
        {
            get { return new ReadOnlyCollection<PinnedRepository>(_applicationService.Account.PinnnedRepositories); }
        }
		
        public MenuViewModel(ISessionService sessionService, IAccountsRepository accountsService)
        {
            _applicationService = sessionService;

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

            DeletePinnedRepositoryCommand = ReactiveCommand.Create();

            DeletePinnedRepositoryCommand.OfType<PinnedRepository>()
                .Subscribe(x =>
                {
                    sessionService.Account.PinnnedRepositories.Remove(x);
                    accountsService.Update(sessionService.Account);
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(_ =>
                {
                    var notifications = sessionService.GitHubClient.Notification.GetAllForCurrent();
                    notifications.ToBackground(x => Notifications = x.Count);

                    var organizations = sessionService.GitHubClient.Organization.GetAllForCurrent();
                    organizations.ToBackground(x => Organizations = x);

                    return Task.WhenAll(notifications, organizations);
                });
        }

//        public ICommand GoToDefaultTopView
//        {
//            get
//            {
//                var startupViewName = AccountsService.ActiveAccount.DefaultStartupView;
//                if (!string.IsNullOrEmpty(startupViewName))
//                {
//                    var props = from p in GetType().GetRuntimeProperties()
//                        let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true).ToList()
//                            where attr.Count == 1
//                        select new { Property = p, Attribute = attr[0] as PotentialStartupViewAttribute};
//
//                    foreach (var p in props)
//                    {
//                        if (string.Equals(startupViewName, p.Attribute.Name))
//                            return p.Property.GetValue(this) as ICommand;
//                    }
//                }
//
//                //Oh no... Look for the last resort DefaultStartupViewAttribute
//                var deprop = (from p in GetType().GetRuntimeProperties()
//                    let attr = p.GetCustomAttributes(typeof(DefaultStartupViewAttribute), true).ToList()
//                    where attr.Count == 1
//                    select new { Property = p, Attribute = attr[0] as DefaultStartupViewAttribute }).FirstOrDefault();
//
//                //That shouldn't happen...
//                if (deprop == null)
//                    return null;
//                var val = deprop.Property.GetValue(this);
//                return val as ICommand;
//            }
//        }
//

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
