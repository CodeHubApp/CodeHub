using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Source;
using GitHubSharp.Models;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

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

        public string Sha { get; private set; }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            PullRequestId = navObject.PullRequestId;
            Sha = navObject.Sha;

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
            public string Sha { get; set; }
            public long PullRequestId { get; set; }
        }
    }
}

