using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Source;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestFilesViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<CommitModel.CommitFileModel> _files = new CollectionViewModel<CommitModel.CommitFileModel>();

        public CollectionViewModel<CommitModel.CommitFileModel> Files
        {
            get { return _files; }
        }

        public long PullRequestId { get; private set; }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public ICommand GoToSourceCommand
        {
            get 
            { 
                return new MvxCommand<CommitModel.CommitFileModel>(x => 
                {
                    var name = x.Filename.Substring(x.Filename.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
                    ShowViewModel<SourceViewModel>(new SourceViewModel.NavObject { Name = name, Path = x.Filename, GitUrl = x.ContentsUrl, ForceBinary = x.Patch == null });
                });
            }
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            PullRequestId = navObject.PullRequestId;

            _files.GroupingFunction = (x) => x.GroupBy(y => {
                var filename = "/" + y.Filename;
                return filename.Substring(0, filename.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
            }).OrderBy(y => y.Key);
        }

        protected override Task Load()
        {
            return Files.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId].GetFiles());
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public long PullRequestId { get; set; }
        }
    }
}

