using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesExploreViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;

        public bool ShowRepositoryDescription
        {
            get { return _applicationService.Account.ShowRepositoryDescriptionInList; }
        }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Repositories { get; private set; }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public IReactiveCommand SearchCommand { get; private set; }

        public RepositoriesExploreViewModel(IApplicationService applicationService, INetworkActivityService networkActivityService)
        {
            _applicationService = applicationService;

            var gotoRepository = new Action<RepositoryItemViewModel>(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner;
                vm.RepositoryName = x.Name;
                ShowViewModel(vm);
            });

            var repositories = new ReactiveList<RepositorySearchModel.RepositoryModel>();
            Repositories = repositories.CreateDerivedCollection(x => 
                new RepositoryItemViewModel(x.Name, x.Owner.Login, x.Owner.AvatarUrl, 
                                            x.Description, x.StargazersCount, x.ForksCount,
                                            gotoRepository));

            SearchCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.SearchText).Select(x => !string.IsNullOrEmpty(x)),
                async t =>
            {
                try
                {
                    var request = applicationService.Client.Repositories.SearchRepositories(new[] { SearchText }, new string[] { });
                    request.UseCache = false;
                    var response = await applicationService.Client.ExecuteAsync(request);
                    repositories.Reset(response.Data.Items);
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

