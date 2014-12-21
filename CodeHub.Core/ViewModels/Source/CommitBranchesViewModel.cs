using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using System;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitBranchesViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IReadOnlyReactiveList<BranchItemViewModel> Branches { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public CommitBranchesViewModel(IApplicationService applicationService)
        {
            Title = "Commit Branch";

            var branches = new ReactiveList<Octokit.Branch>();
            Branches = branches.CreateDerivedCollection(
                x => new BranchItemViewModel(x.Name, () =>
                {
                    var vm = this.CreateViewModel<CommitsViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    vm.Branch = x.Name;
                    NavigateTo(vm);
                }),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
                branches.Reset(await applicationService.GitHubClient.Repository.GetAllBranches(RepositoryOwner, RepositoryName)));
        }
    }
}

