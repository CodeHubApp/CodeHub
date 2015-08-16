using System;
using System.Linq;
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
using CodeHub.Core.ViewModels.Search;
using System.Reflection;
using System.Xml.Linq;
using CodeHub.Core.ViewModels.Accounts;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly ISessionService _sessionService;
		private int _notifications;

		public int Notifications
        {
            get { return _notifications; }
            private set { this.RaiseAndSetIfChanged(ref _notifications, value); }
        }

        private IReadOnlyList<Octokit.Organization> _organizations;
        public IReadOnlyList<Octokit.Organization> Organizations
        {
            get { return _organizations; }
            private set { this.RaiseAndSetIfChanged(ref _organizations, value); }
        }
		
        public GitHubAccount Account
        {
            get { return _sessionService.Account; }
        }

        private readonly ObservableAsPropertyHelper<GitHubAvatar> _avatar;
        public GitHubAvatar Avatar
        {
            get { return new GitHubAvatar(Account.AvatarUrl); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> DeletePinnedRepositoryCommand { get; private set; }

        public IReadOnlyList<PinnedRepository> PinnedRepositories 
        {
            get { return new ReadOnlyCollection<PinnedRepository>(_sessionService.Account.PinnnedRepositories); }
        }
		
        public MenuViewModel(ISessionService sessionService, IAccountsRepository accountsService)
        {
            _sessionService = sessionService;

            this.WhenAnyValue(x => x.Account)
                .Select(x => new GitHubAvatar(x.AvatarUrl))
                .ToProperty(this, x => x.Avatar, out _avatar);

            GoToNotificationsCommand = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = this.CreateViewModel<NotificationsViewModel>();
                vm.NotificationCount.Subscribe(x => Notifications = x);
                NavigateTo(vm);
            });

            GoToAccountsCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<AccountsViewModel>()));

            GoToProfileCommand = ReactiveCommand.Create();
            GoToProfileCommand
                .Select(_ => this.CreateViewModel<UserViewModel>())
                .Select(x => x.Init(Account.Username))
                .Subscribe(NavigateTo);

            GoToMyIssuesCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<MyIssuesViewModel>()));
     
            GoToRepositoryCommand = ReactiveCommand.Create();
            GoToRepositoryCommand.OfType<RepositoryIdentifier>()
                .Select(x => this.CreateViewModel<RepositoryViewModel>().Init(x.Owner, x.Name))
                .Subscribe(NavigateTo);

            GoToSettingsCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<SettingsViewModel>()));

            GoToNewsCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<NewsViewModel>()));

            GoToOrganizationsCommand = ReactiveCommand.Create();
            GoToOrganizationsCommand
                .Select(_ => this.CreateViewModel<OrganizationsViewModel>())
                .Select(x => x.Init(Account.Username))
                .Subscribe(NavigateTo);

            GoToTrendingRepositoriesCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<RepositoriesTrendingViewModel>()));

            GoToExploreRepositoriesCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<ExploreViewModel>()));

            GoToOrganizationEventsCommand = ReactiveCommand.Create();
            GoToOrganizationEventsCommand
                .OfType<Octokit.Organization>()
                .Select(x => this.CreateViewModel<UserEventsViewModel>().Init(x.Login))
                .Subscribe(NavigateTo); 

            GoToOrganizationCommand = ReactiveCommand.Create();
            GoToOrganizationCommand
                .OfType<Octokit.Organization>()
                .Select(x => this.CreateViewModel<OrganizationViewModel>().Init(x.Login))
                .Subscribe(NavigateTo);

            GoToOwnedRepositoriesCommand = ReactiveCommand.Create();
            GoToOwnedRepositoriesCommand
                .Select(_ => this.CreateViewModel<UserRepositoriesViewModel>())
                .Select(x => x.Init(Account.Username))
                .Subscribe(NavigateTo);

            GoToStarredRepositoriesCommand = ReactiveCommand.Create().WithSubscription(
                _ => NavigateTo(this.CreateViewModel<RepositoriesStarredViewModel>()));

            GoToWatchedRepositoriesCommand = ReactiveCommand.Create().WithSubscription(
                _ => NavigateTo(this.CreateViewModel<RepositoriesWatchedViewModel>()));

            GoToPublicGistsCommand = ReactiveCommand.Create().WithSubscription(
                _ => NavigateTo(this.CreateViewModel<PublicGistsViewModel>()));

            GoToStarredGistsCommand = ReactiveCommand.Create().WithSubscription(
                _ => NavigateTo(this.CreateViewModel<StarredGistsViewModel>()));

            GoToMyGistsCommand = ReactiveCommand.Create();
            GoToMyGistsCommand
                .Select(_ => this.CreateViewModel<UserGistsViewModel>())
                .Select(x => x.Init(Account.Username))
                .Subscribe(NavigateTo);
  
            GoToMyEvents = ReactiveCommand.Create();
            GoToMyEvents
                .Select(_ => this.CreateViewModel<UserEventsViewModel>())
                .Select(x => x.Init(Account.Username))
                .Subscribe(NavigateTo); 
                
            GoToFeedbackCommand = ReactiveCommand.Create();
            GoToFeedbackCommand.Subscribe(_ => {
                var vm = sessionService.Account.IsEnterprise
                    ? (IBaseViewModel)this.CreateViewModel<EnterpriseSupportViewModel>()
                    : this.CreateViewModel<SupportViewModel>();
                NavigateTo(vm);
            });

            DeletePinnedRepositoryCommand = ReactiveCommand.Create();

            DeletePinnedRepositoryCommand.OfType<PinnedRepository>()
                .Subscribe(x => {
                    sessionService.Account.PinnnedRepositories.Remove(x);
                    accountsService.Update(sessionService.Account);
                });

            ActivateCommand = ReactiveCommand.Create();
            ActivateCommand.Subscribe(x => {
                var startupViewModel = sessionService.StartupViewModel;
                sessionService.StartupViewModel = null;
                if (startupViewModel != null)
                    NavigateTo(startupViewModel);
                else
                    GoToDefaultTopView.ExecuteIfCan();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(_ => {
                var notifications = sessionService.GitHubClient.Notification.GetAllForCurrent();
                notifications.ToBackground(x => Notifications = x.Count);

                var organizations = sessionService.GitHubClient.Organization.GetAllForCurrent();
                organizations.ToBackground(x => Organizations = x);

                return Task.WhenAll(notifications, organizations);
            });
        }

        public IReactiveCommand GoToDefaultTopView
        {
            get
            {
                var startupViewName = Account.DefaultStartupView;
                if (!string.IsNullOrEmpty(startupViewName))
                {
                    var props = from p in GetType().GetRuntimeProperties()
                       let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true).ToList()
                       where attr.Count == 1
                       let a = attr[0] as PotentialStartupViewAttribute
                       where string.Equals(startupViewName, a.Name)
                       select p.GetValue(this) as IReactiveCommand;

                    var cmd = props.FirstOrDefault(x => x != null);

                    if (cmd != null)
                        return cmd;
                }

                return GoToNewsCommand;
            }
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

		[PotentialStartupView("News")]
        public IReactiveCommand<object> GoToNewsCommand { get; private set; }

		public IReactiveCommand<object> GoToSettingsCommand { get; private set; }

		public IReactiveCommand<object> GoToRepositoryCommand { get; private set; }

        public IReactiveCommand<object> GoToFeedbackCommand { get; private set; }

        public IReactiveCommand<object> ActivateCommand { get; private set; }

    }
}
