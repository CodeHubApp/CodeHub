using System;
using ReactiveUI;
using CodeHub.Core.Services;
using Octokit;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleasesViewModel : BaseListViewModel<Release, ReleaseItemViewModel>
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public ReleasesViewModel(ISessionService applicationService)
        {
            Title = "Releases";
            Items = InternalItems.CreateDerivedCollection(CreateItemViewModel, x => !x.Draft);
            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                InternalItems.Reset(await applicationService.GitHubClient.Release.GetAll(RepositoryOwner, RepositoryName)));
        }

        private ReleaseItemViewModel CreateItemViewModel(Release release)
        {
            var releaseItem = new ReleaseItemViewModel(release);
            releaseItem.GoToCommand.Subscribe(_ => {
                var vm = this.CreateViewModel<ReleaseViewModel>();
                vm.Init(RepositoryOwner, RepositoryName, release.Id);
                NavigateTo(vm);
            });
            return releaseItem;
        }

        public ReleasesViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}

