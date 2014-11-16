using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitBranchesViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IReadOnlyReactiveList<BranchModel> Branches { get; private set; }

        public IReactiveCommand<object> GoToBranchCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public CommitBranchesViewModel(IApplicationService applicationService)
        {
            Title = "Commit Branch";

            var branches = new ReactiveList<BranchModel>();
            Branches = branches.CreateDerivedCollection(
                x => x,
                x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToBranchCommand = ReactiveCommand.Create();
            GoToBranchCommand.OfType<BranchModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<ChangesetsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Branch = x.Name;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                branches.LoadAll(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetBranches()));
        }
    }
}

