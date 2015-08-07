using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using Humanizer;

namespace CodeHub.Core.ViewModels.Search
{
    public class ExploreViewModel : BaseViewModel
    {
        public bool ShowRepositoryDescription { get; private set; }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Repositories { get; private set; }

        public IReadOnlyReactiveList<UserItemViewModel> Users { get; private set; }

        private SearchType _searchFilter;
        public SearchType SearchFilter
        {
            get { return _searchFilter; }
            set { this.RaiseAndSetIfChanged(ref _searchFilter, value); }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public IReactiveCommand<Unit> SearchCommand { get; private set; }

        public ExploreViewModel(ISessionService applicationService)
        {
            ShowRepositoryDescription = applicationService.Account.ShowRepositoryDescriptionInList;

            Title = "Explore";

            var gotoRepository = new Action<RepositoryItemViewModel>(x => {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.Init(x.Owner, x.Name, x.Repository);
                NavigateTo(vm);
            });

            var repositories = new ReactiveList<Octokit.Repository>();
            Repositories = repositories.CreateDerivedCollection(x => 
                new RepositoryItemViewModel(x, true, gotoRepository));

            var users = new ReactiveList<Octokit.User>();
            Users = users.CreateDerivedCollection(x => 
                new UserItemViewModel(x.Login, x.AvatarUrl, false, () => {
                    var vm = this.CreateViewModel<UserViewModel>();
                    vm.Init(x.Login, x);
                    NavigateTo(vm);
                }));

            this.WhenAnyValue(x => x.SearchFilter)
                .DistinctUntilChanged()
                .Subscribe(_ => {
                    SearchText = string.Empty;
                    users.Clear();
                    repositories.Clear();
                });

            var canSearch = this.WhenAnyValue(x => x.SearchText).Select(x => !string.IsNullOrEmpty(x));
            SearchCommand = ReactiveCommand.CreateAsyncTask(canSearch, async t => {
                try
                {
                    users.Clear();
                    repositories.Clear();

                    if (SearchFilter == SearchType.Repositories)
                    {
                        var request = new Octokit.SearchRepositoriesRequest(SearchText);
                        var response = await applicationService.GitHubClient.Search.SearchRepo(request);
                        repositories.Reset(response.Items);
                    }
                    else if (SearchFilter == SearchType.Users)
                    {
                        var request = new Octokit.SearchUsersRequest(SearchText);
                        var response = await applicationService.GitHubClient.Search.SearchUsers(request);
                        users.Reset(response.Items);
                    }
                }
                catch (Exception e)
                {
                    var msg = string.Format("Unable to search for {0}. Please try again.", SearchFilter.Humanize());
                    throw new Exception(msg, e);
                }
            });
        }

        public enum SearchType
        {
            Repositories = 0,
            Users
        }
    }
}

