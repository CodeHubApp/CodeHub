using System;
using CodeHub.Core.Services;
using Octokit;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Humanizer;

namespace CodeHub.Core.ViewModels.Search
{
    public class RepositoryExploreViewModel : BaseSearchableListViewModel<Repository, RepositoryItemViewModel>
    {
        public bool ShowRepositoryDescription { get; }

        public IReactiveCommand<Unit> SearchCommand { get; }

        public RepositoryExploreViewModel(ISessionService applicationService)
        {
            ShowRepositoryDescription = applicationService.Account.ShowRepositoryDescriptionInList;
            Title = "Explore Repositories";

            var gotoRepository = new Action<RepositoryItemViewModel>(x => {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.Init(x.Owner, x.Name, x.Repository);
                NavigateTo(vm);
            });

            Items = InternalItems.CreateDerivedCollection(x => 
                new RepositoryItemViewModel(x, true, gotoRepository));

            var canSearch = this.WhenAnyValue(x => x.SearchKeyword).Select(x => !string.IsNullOrEmpty(x));
            SearchCommand = ReactiveCommand.CreateAsyncTask(canSearch, async t => {
                try
                {
                    InternalItems.Clear();
                    var request = new SearchRepositoriesRequest(SearchKeyword);
                    var response = await applicationService.GitHubClient.Search.SearchRepo(request);
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

