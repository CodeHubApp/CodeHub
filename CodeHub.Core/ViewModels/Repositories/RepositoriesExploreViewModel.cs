using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesExploreViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private string _searchText;

        public bool ShowRepositoryDescription
        {
            get { return _applicationService.Account.ShowRepositoryDescriptionInList; }
        }

        public ReactiveCollection<RepositorySearchModel.RepositoryModel> Repositories { get; private set; }

        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public IReactiveCommand GoToRepositoryCommand { get; private set; }

        public IReactiveCommand SearchCommand { get; private set; }

        public RepositoriesExploreViewModel(IApplicationService applicationService, INetworkActivityService networkActivityService)
        {
            _applicationService = applicationService;
            Repositories = new ReactiveCollection<RepositorySearchModel.RepositoryModel>();

            SearchCommand = new ReactiveCommand(this.WhenAnyValue(x => x.SearchText, x => !string.IsNullOrEmpty(x)));
            SearchCommand.IsExecuting.Skip(1).Subscribe(x => 
            {
                if (x)
                    networkActivityService.PushNetworkActive();
                else
                    networkActivityService.PopNetworkActive();
            });
            SearchCommand.RegisterAsyncTask(async t =>
            {
                try
                {
                    var request = applicationService.Client.Repositories.SearchRepositories(new[] { SearchText }, new string[] { });
                    request.UseCache = false;
                    var response = await applicationService.Client.ExecuteAsync(request);
                    Repositories.Reset(response.Data.Items);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to search for repositories. Please try again.", e);
                }
            });

            GoToRepositoryCommand = new ReactiveCommand();
            GoToRepositoryCommand.OfType<RepositorySearchModel.RepositoryModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner.Login;
                vm.RepositoryName = x.Name;
                ShowViewModel(vm);
            });
        }
    }
}

