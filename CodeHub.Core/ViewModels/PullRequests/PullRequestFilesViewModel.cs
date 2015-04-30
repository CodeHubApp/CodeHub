using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using System.Reactive;
using Octokit;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestFilesViewModel : BaseViewModel, ILoadableViewModel
    {
        public IReadOnlyReactiveList<CommitedFileItemViewModel> Files { get; private set; }

        public IReadOnlyReactiveList<PullRequestReviewComment> Comments { get; private set; }

        public int PullRequestId { get; private set; }

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public PullRequestFilesViewModel(ISessionService applicationService)
        {
            Title = "Files Changed";

            var files = new ReactiveList<PullRequestFile>();
            Files = files.CreateDerivedCollection(x => new CommitedFileItemViewModel(x, y => {
                if (x.Patch == null)
                {
                    var vm = this.CreateViewModel<SourceViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    vm.Branch = y.Ref;
                    vm.Name = y.Name;
                    vm.Path = x.FileName;
                    vm.GitUrl = x.ContentsUrl.AbsoluteUri;
                    vm.HtmlUrl = x.BlobUrl.AbsoluteUri;
                    vm.ForceBinary = true;
                    NavigateTo(vm);
                }
                else
                {
                    NavigateTo(this.CreateViewModel<PullRequestDiffViewModel>().Init(this, x));
                }
            }));

            var comments = new ReactiveList<PullRequestReviewComment>();
            Comments = comments.CreateDerivedCollection(x => x);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                applicationService.GitHubClient.Repository.PullRequest.Comment.GetAll(RepositoryOwner, RepositoryName, PullRequestId)
                    .ToBackground(x => comments.Reset(x));
                files.Reset(await applicationService.GitHubClient.Repository.PullRequest.Files(RepositoryOwner, RepositoryName, PullRequestId));
            });
        }

        public PullRequestFilesViewModel Init(string repositoryOwner, string repositoryName, int pullRequestId)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            PullRequestId = pullRequestId;
            return this;
        }
    }
}

