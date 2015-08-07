using System;
using ReactiveUI;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Organizations;
using System.Reactive;
using CodeHub.Core.ViewModels.Users;
using Octokit;
using System.Threading.Tasks;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public abstract class BaseUsersViewModel : BaseViewModel, IPaginatableViewModel, IProvidesSearchKeyword
    {
        private readonly ReactiveList<User> _users = new ReactiveList<User>();

        public IReadOnlyReactiveList<UserItemViewModel> Users { get; private set; }

        protected ISessionService SessionService { get; private set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; protected set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected BaseUsersViewModel(ISessionService sessionService)
        {
            SessionService = sessionService;

            Users = _users.CreateDerivedCollection(x => {
                var isOrg = x.Type.HasValue && x.Type.Value == AccountType.Organization;
                return new UserItemViewModel(x.Login, x.AvatarUrl, isOrg, () => {
                    if (isOrg)
                    {
                        var vm = this.CreateViewModel<OrganizationViewModel>();
                        vm.Init(x.Login);
                        NavigateTo(vm);
                    }
                    else
                    {
                        var vm = this.CreateViewModel<UserViewModel>();
                        vm.Init(x.Login, x);
                        NavigateTo(vm);
                    }
                });
            },
            x => x.Login.StartsWith(SearchKeyword ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                _users.Reset(await RetrieveUsers());
            });
        }

        private async Task<IReadOnlyList<User>> RetrieveUsers(int page = 1)
        {
            var connection = SessionService.GitHubClient.Connection;
            var parameters = new Dictionary<string, string>();
            parameters["page"] = page.ToString();
            parameters["per_page"] = 100.ToString();
            var ret = await connection.Get<IReadOnlyList<User>>(RequestUri, parameters, "application/json");

            if (ret.HttpResponse.ApiInfo.Links.ContainsKey("next"))
            {
                LoadMoreCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                    _users.AddRange(await RetrieveUsers(page + 1));
                });
            }
            else
            {
                LoadMoreCommand = null;
            }

            return ret.Body;
        }

        protected abstract Uri RequestUri { get; }
    }
}
