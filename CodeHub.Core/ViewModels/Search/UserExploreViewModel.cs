using System;
using CodeHub.Core.Services;
using ReactiveUI;
using Octokit;
using CodeHub.Core.ViewModels.Users;
using System.Reactive;
using System.Reactive.Linq;
using Humanizer;

namespace CodeHub.Core.ViewModels.Search
{
    public class UserExploreViewModel : BaseSearchableListViewModel<User, UserItemViewModel>
    {
        public IReactiveCommand<Unit> SearchCommand { get; }

        public UserExploreViewModel(ISessionService applicationService)
        {
            Title = "Explore Users";

            Items = InternalItems.CreateDerivedCollection(x => 
                new UserItemViewModel(x.Login, x.AvatarUrl, false, () => {
                    var vm = this.CreateViewModel<UserViewModel>();
                    vm.Init(x.Login, x);
                    NavigateTo(vm);
                }));

            var canSearch = this.WhenAnyValue(x => x.SearchKeyword).Select(x => !string.IsNullOrEmpty(x));
            SearchCommand = ReactiveCommand.CreateAsyncTask(canSearch, async t => {
                try
                {
                    InternalItems.Clear();
                    var request = new SearchUsersRequest(SearchKeyword);
                    var response = await applicationService.GitHubClient.Search.SearchUsers(request);
                    InternalItems.Reset(response.Items);
                }
                catch (Exception e)
                {
                    var msg = string.Format("Unable to search for {0}. Please try again.", SearchKeyword.Humanize());
                    throw new Exception(msg, e);
                }
            });
        }
    }
}

