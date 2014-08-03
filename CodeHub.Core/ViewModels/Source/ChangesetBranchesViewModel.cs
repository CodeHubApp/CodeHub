using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class ChangesetBranchesViewModel : BaseViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public ReactiveList<BranchModel> Branches { get; private set; }

        public IReactiveCommand<object> GoToBranchCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public ChangesetBranchesViewModel(IApplicationService applicationService)
        {
            Branches = new ReactiveList<BranchModel>();

            GoToBranchCommand = ReactiveCommand.Create();
            GoToBranchCommand.OfType<BranchModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<ChangesetsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                Branches.SimpleCollectionLoad(
                    applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetBranches(), t as bool?));
        }
    }
}

