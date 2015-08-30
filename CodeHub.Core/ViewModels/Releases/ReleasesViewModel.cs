using System;
using ReactiveUI;
using CodeHub.Core.Services;
using Octokit;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleasesViewModel : BaseListViewModel<Release, ReleaseItemViewModel>
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

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
            releaseItem.GoToCommand.Subscribe(_ => 
            {
                var vm = this.CreateViewModel<ReleaseViewModel>();
                vm.RepositoryName = RepositoryName;
                vm.RepositoryOwner = RepositoryOwner;
                vm.ReleaseId = release.Id;
                NavigateTo(vm);
            });

            return releaseItem;
        }
    }
}

