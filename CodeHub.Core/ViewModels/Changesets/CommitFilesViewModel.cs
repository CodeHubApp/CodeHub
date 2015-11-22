using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitFilesViewModel : BaseViewModel
    {
        private readonly IReactiveList<Octokit.GitHubCommitFile> _files = new ReactiveList<Octokit.GitHubCommitFile>();

        public IReadOnlyReactiveList<CommitedFileItemViewModel> Files { get; }

        public string RepositoryName { get; private set; }

        public string RepositoryOwner { get; private set; }

        public string CommitSha { get; private set; }

        public CommitFilesViewModel()
        {
            Files = _files.CreateDerivedCollection(x => new CommitedFileItemViewModel(x, y => {
                if (x.Patch == null)
                {
                    var vm = this.CreateViewModel<SourceViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    vm.Branch = y.Ref;
                    vm.Name = y.Name;
                    vm.Path = x.Filename;
                    vm.GitUrl = x.ContentsUrl;
                    vm.HtmlUrl = x.BlobUrl;
                    vm.ForceBinary = true;
                    NavigateTo(vm);
                }
                else
                {
                    var vm = this.CreateViewModel<ChangesetDiffViewModel>();
                    vm.Username = RepositoryOwner;
                    vm.Repository = RepositoryName;
                    vm.Branch = CommitSha;
                    vm.Filename = x.Filename;
                    NavigateTo(vm);
                }
            }));
        }

        internal CommitFilesViewModel Init(string repoOwner, string repoName, string commitSha, string title, IEnumerable<Octokit.GitHubCommitFile> files)
        {
            Title = title;
            RepositoryOwner = repoOwner;
            RepositoryName = repoName;
            CommitSha = commitSha;
            _files.Reset(files);
            return this;
        }
    }
}

