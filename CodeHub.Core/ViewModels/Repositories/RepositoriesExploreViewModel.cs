using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesExploreViewModel : BaseViewModel
    {
        public bool ShowRepositoryDescription { get; private set; }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Repositories { get; private set; }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public IReactiveCommand<Unit> SearchCommand { get; private set; }

        public RepositoriesExploreViewModel(IApplicationService applicationService, INetworkActivityService networkActivityService)
        {
            ShowRepositoryDescription = applicationService.Account.ShowRepositoryDescriptionInList;

            Title = "Explore";

            var gotoRepository = new Action<RepositoryItemViewModel>(x =>
            {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner;
                vm.RepositoryName = x.Name;
                NavigateTo(vm);
            });

            var repositories = new ReactiveList<Octokit.Repository>();
            Repositories = repositories.CreateDerivedCollection(x => 
                new RepositoryItemViewModel(x.Name, x.Owner.Login, x.Owner.AvatarUrl, 
                                            x.Description, x.StargazersCount, x.ForksCount,
                                            true, gotoRepository));

            SearchCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.SearchText).Select(x => !string.IsNullOrEmpty(x)),
                async t =>
            {
                repositories.Clear();

                try
                {
                    var request = new Octokit.SearchRepositoriesRequest(SearchText);
                    var response = await applicationService.GitHubClient.Search.SearchRepo(request);
                    repositories.Reset(response.Items);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to search for repositories. Please try again.", e);
                }
            });
            
            SearchCommand.IsExecuting.Skip(1).Subscribe(x => 
            {
                if (x)
                    networkActivityService.PushNetworkActive();
                else
                    networkActivityService.PopNetworkActive();
            });
        }
    }
}

