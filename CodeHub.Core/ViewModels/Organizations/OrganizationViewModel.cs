using System;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.ViewModels.Activity;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : BaseViewModel, ILoadableViewModel
	{
        private string _username;
        public string Username
        {
            get { return _username; }
            private set { this.RaiseAndSetIfChanged(ref _username, value); }
        }

        private Octokit.Organization _userModel;
        public Octokit.Organization Organization
        {
            get { return _userModel; }
            private set { this.RaiseAndSetIfChanged(ref _userModel, value); }
        }

        private bool _canViewTeams;
        public bool CanViewTeams
        {
            get { return _canViewTeams; }
            private set { this.RaiseAndSetIfChanged(ref _canViewTeams, value); }
        }

        private readonly ObservableAsPropertyHelper<GitHubAvatar> _avatar;
        public GitHubAvatar Avatar
        {
            get { return _avatar.Value; }
        }

        public IReactiveCommand<object> GoToMembersCommand { get; private set; }

        public IReactiveCommand<object> GoToTeamsCommand { get; private set; }

        public IReactiveCommand<object> GoToFollowersCommand { get; private set; }

        public IReactiveCommand<object> GoToFollowingCommand { get; private set; }

        public IReactiveCommand<object> GoToEventsCommand { get; private set; }

        public IReactiveCommand<object> GoToGistsCommand { get; private set; }

        public IReactiveCommand<object> GoToRepositoriesCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public OrganizationViewModel(ISessionService applicationService)
        {
            this.WhenAnyValue(x => x.Organization, x => x.Username, 
                (x, y) => x == null ? y : (string.IsNullOrEmpty(x.Name) ? x.Login : x.Name))
                .Select(x => x ?? "Organization")
                .Subscribe(x => Title = x);

            this.WhenAnyValue(x => x.Organization.AvatarUrl)
                .Select(x => new GitHubAvatar(x))
                .ToProperty(this, x => x.Avatar, out _avatar);

            GoToMembersCommand = ReactiveCommand.Create();
            GoToMembersCommand
                .Select(_ => this.CreateViewModel<OrganizationMembersViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToTeamsCommand = ReactiveCommand.Create();
            GoToTeamsCommand
                .Select(_ => this.CreateViewModel<TeamsViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToFollowersCommand = ReactiveCommand.Create();
            GoToFollowersCommand
                .Select(_ => this.CreateViewModel<UserFollowersViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToFollowingCommand = ReactiveCommand.Create();
            GoToFollowingCommand
                .Select(_ => this.CreateViewModel<UserFollowingsViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToEventsCommand = ReactiveCommand.Create();
            GoToEventsCommand
                .Select(_ => this.CreateViewModel<UserEventsViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToGistsCommand = ReactiveCommand.Create();
            GoToGistsCommand
                .Select(_ => this.CreateViewModel<UserGistsViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToRepositoriesCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<OrganizationRepositoriesViewModel>();
                vm.Name = Username;
                NavigateTo(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                applicationService.GitHubClient.Organization.Team.GetAll(Username)
                    .ToBackground(x => CanViewTeams = true);

                Organization = await applicationService.GitHubClient.Organization.Get(Username);
            });
        }

        public OrganizationViewModel Init(string organizationName, Octokit.Organization organization = null)
        {
            Username = organizationName;
            Organization = organization;
            return this;
        }
	}
}

