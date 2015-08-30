using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System;
using Octokit;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitBranchesViewModel : BaseSearchableListViewModel<Branch, BranchItemViewModel>
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public CommitBranchesViewModel(ISessionService applicationService)
        {
            Title = "Commit Branch";

            Items = InternalItems.CreateDerivedCollection(
                x => new BranchItemViewModel(x.Name, () => {
                    var vm = this.CreateViewModel<CommitsViewModel>();
                    NavigateTo(vm.Init(RepositoryOwner, RepositoryName, x.Name));
                }),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
                InternalItems.Reset(await applicationService.GitHubClient.Repository.GetAllBranches(RepositoryOwner, RepositoryName)));
        }

        public CommitBranchesViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}

