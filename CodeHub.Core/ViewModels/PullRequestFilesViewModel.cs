using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class PullRequestFilesViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<CommitModel.CommitFileModel> _files = new CollectionViewModel<CommitModel.CommitFileModel>();

        public CollectionViewModel<CommitModel.CommitFileModel> Files
        {
            get { return _files; }
        }

        public ulong PullRequestId { get; private set; }
        public string Username { get; private set; }
        public string Repository { get; private set; }

        public PullRequestFilesViewModel(string username, string repository, ulong pullRequestId)
        {
            Username = username;
            Repository = repository;
            PullRequestId = pullRequestId;

            _files.GroupingFunction = (x) => x.GroupBy(y => {
                var filename = "/" + y.Filename;
                return filename.Substring(0, filename.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
            }).OrderBy(y => y.Key);
        }

        public Task Load(bool forceDataRefresh)
        {
            return Files.SimpleCollectionLoad(Application.Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId].GetFiles(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public ulong PullRequestId { get; set; }
        }
    }
}

