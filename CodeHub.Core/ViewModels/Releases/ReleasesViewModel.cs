using System;
using Xamarin.Utilities.Core.ViewModels;
using ReactiveUI;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using Xamarin.Utilities.Core;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleasesViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public IReactiveCommand LoadCommand { get; private set; }

        public IReadOnlyReactiveList<ReleaseItemViewModel> Releases { get; private set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public ReleasesViewModel(IApplicationService applicationService)
        {
            var releases = new ReactiveList<ReleaseModel>();
            Releases = releases.CreateDerivedCollection(
                x => {
                    var published = x.PublishedAt.HasValue ? x.PublishedAt.Value : x.CreatedAt;
                    return new ReleaseItemViewModel(x.Id, x.Name, published, _ => {
                        var vm = CreateViewModel<ReleaseViewModel>();
                        vm.RepositoryName = RepositoryName;
                        vm.RepositoryOwner = RepositoryOwner;
                        vm.ReleaseId = x.Id;
                        vm.ReleaseModel = x;
                        ShowViewModel(vm);
                    });
                },
                x => !x.Draft && x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                releases.SimpleCollectionLoad(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetReleases(), t as bool?));
        }
    }
}

