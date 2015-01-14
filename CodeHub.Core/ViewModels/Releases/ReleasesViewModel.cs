using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleasesViewModel : BaseViewModel, ILoadableViewModel
    {
        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReadOnlyReactiveList<ReleaseItemViewModel> Releases { get; private set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public ReleasesViewModel(IApplicationService applicationService)
        {
            Title = "Releases";

            var releases = new ReactiveList<Octokit.Release>();
            Releases = releases.CreateDerivedCollection(x => 
            {
                var published = x.PublishedAt.HasValue ? x.PublishedAt.Value : x.CreatedAt;
                var name = string.IsNullOrEmpty(x.Name) ? x.TagName : x.Name;
                return new ReleaseItemViewModel(x.Id, name, published, _ => {
                    var vm = this.CreateViewModel<ReleaseViewModel>();
                    vm.RepositoryName = RepositoryName;
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.ReleaseId = x.Id;
                    NavigateTo(vm);
                });
            },
            x => !x.Draft);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                releases.Reset(await applicationService.GitHubClient.Release.GetAll(RepositoryOwner, RepositoryName)));
        }
    }
}

