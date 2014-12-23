using System;
using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Services;
using System.Reactive.Subjects;
using Octokit;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitCommentViewModel : MarkdownComposerViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Subject<CommitComment> _commendAdded = new Subject<CommitComment>();

        public string Node { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IObservable<CommitComment> CommentAdded { get { return _commendAdded; } }

        public CommitCommentViewModel(IApplicationService applicationService) 
        {
            _applicationService = applicationService;
        }

        protected override async System.Threading.Tasks.Task Save()
        {
            var commitComment = new NewCommitComment(Text);
            var comment = await _applicationService.GitHubClient.Repository.RepositoryComments.Create(RepositoryOwner, RepositoryName, Node, commitComment);
            _commendAdded.OnNext(comment);
        }
    }
}

