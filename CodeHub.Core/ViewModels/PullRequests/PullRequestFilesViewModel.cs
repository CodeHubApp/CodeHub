using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using System.Reactive;
using Octokit;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestFilesViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly ISubject<PullRequestReviewComment> _commentCreatedObservable = new Subject<PullRequestReviewComment>();

        public IObservable<PullRequestReviewComment> CommentCreated
        {
            get { return _commentCreatedObservable.AsObservable(); }
        }

        public IReadOnlyReactiveList<CommitedFileItemViewModel> Files { get; }

        public IReadOnlyReactiveList<PullRequestReviewComment> Comments { get; }

        public int PullRequestId { get; private set; }

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public string HeadSha { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public PullRequestFilesViewModel(ISessionService applicationService)
        {
            Title = "Files Changed";

            var comments = new ReactiveList<PullRequestReviewComment>();
            Comments = comments.CreateDerivedCollection(x => x);

            _commentCreatedObservable.Subscribe(comments.Add);

            var files = new ReactiveList<PullRequestFile>();
            Files = files.CreateDerivedCollection(x => new CommitedFileItemViewModel(x, Comments.Count(y => string.Equals(y.Path, x.FileName)), y => {
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
                    var vm = this.CreateViewModel<PullRequestDiffViewModel>();
                    vm.Init(this, x);
                    vm.CommentCreated.Subscribe(_commentCreatedObservable);
                    NavigateTo(vm);
                }
            }), signalReset: comments.CountChanged);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                applicationService.GitHubClient.Repository.PullRequest.Comment.GetAll(RepositoryOwner, RepositoryName, PullRequestId)
                    .ToBackground(x => comments.Reset(x));
                files.Reset(await applicationService.GitHubClient.Repository.PullRequest.Files(RepositoryOwner, RepositoryName, PullRequestId));
            });
        }

        public PullRequestFilesViewModel Init(string repositoryOwner, string repositoryName, int pullRequestId, string headSha)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            PullRequestId = pullRequestId;
            HeadSha = headSha;
            return this;
        }
    }
}

