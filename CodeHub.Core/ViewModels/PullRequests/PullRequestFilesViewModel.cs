using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using GitHubSharp.Models;
using System.Reactive;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestFilesViewModel : BaseViewModel, ILoadableViewModel
    {
        public IReadOnlyReactiveList<CommitedFileItemViewModel> Files { get; private set; }

        public long PullRequestId { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public PullRequestFilesViewModel(ISessionService applicationService)
        {
            Title = "Files";

            var files = new ReactiveList<CommitModel.CommitFileModel>();
            Files = files.CreateDerivedCollection(x => new CommitedFileItemViewModel(x, y =>
            {
                var vm = this.CreateViewModel<SourceViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Name = y.Name;
                vm.Path = x.Filename;
                vm.GitUrl = x.ContentsUrl;
                vm.ForceBinary = x.Patch == null;
                NavigateTo(vm);
            }));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].PullRequests[PullRequestId].GetFiles();
                var response = await applicationService.Client.ExecuteAsync(request);
                files.Reset(response.Data);
            });
        }
    }
}

