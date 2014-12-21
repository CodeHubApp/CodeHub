using System;
using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Services;
using System.Reactive.Subjects;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueCommentViewModel : MarkdownComposerViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Subject<Octokit.IssueComment> _commendAdded = new Subject<Octokit.IssueComment>();

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int Id { get; set; }

        public IObservable<Octokit.IssueComment> CommentAdded { get { return _commendAdded; } }

        public IssueCommentViewModel(IApplicationService applicationService) 
        {
            _applicationService = applicationService;
        }

        protected override async System.Threading.Tasks.Task Save()
        {
            var comment = await _applicationService.GitHubClient.Issue.Comment.Create(RepositoryOwner, RepositoryName, Id, Text);
            _commendAdded.OnNext(comment);
        }
    }
}

