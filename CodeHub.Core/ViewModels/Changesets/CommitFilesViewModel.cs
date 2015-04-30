using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitFilesViewModel : BaseViewModel
    {
        private IReadOnlyList<CommitedFileItemViewModel> _files;
        public IReadOnlyList<CommitedFileItemViewModel> Files
        {
            get { return _files; }
            set { this.RaiseAndSetIfChanged(ref _files, value); }
        }

        internal CommitFilesViewModel Init(string repoOwner, string repoName, string commitSha, string title, IEnumerable<Octokit.GitHubCommitFile> commitFiles)
        {
            Title = title;

            var files = commitFiles.Select(x => new CommitedFileItemViewModel(x, y => {
                if (x.Patch == null)
                {
                    var vm = this.CreateViewModel<SourceViewModel>();
                    vm.RepositoryOwner = repoOwner;
                    vm.RepositoryName = repoName;
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
                    vm.Username = repoOwner;
                    vm.Repository = repoName;
                    vm.Branch = commitSha;
                    vm.Filename = x.Filename;
                    NavigateTo(vm);
                }
            })).ToList();

            Files = new ReadOnlyCollection<CommitedFileItemViewModel>(files);
            return this;
        }
    }
}

