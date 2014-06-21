using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class ChangesetBranchesViewModel : LoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public ReactiveCollection<BranchModel> Branches { get; private set; }

        public IReactiveCommand GoToBranchCommand { get; private set; }

        public ChangesetBranchesViewModel(IApplicationService applicationService)
        {
            Branches = new ReactiveCollection<BranchModel>();

            GoToBranchCommand = new ReactiveCommand();
            GoToBranchCommand.OfType<BranchModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<ChangesetsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            LoadCommand.RegisterAsyncTask(t =>
                Branches.SimpleCollectionLoad(
                    applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetBranches(), t as bool?));
        }
    }
}

