using System;
using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Services;
using System.Reactive.Subjects;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommentViewModel : MarkdownComposerViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Subject<Octokit.PullRequestReviewComment> _commendAdded = new Subject<Octokit.PullRequestReviewComment>();

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int Id { get; set; }

        public IObservable<Octokit.PullRequestReviewComment> CommentAdded { get { return _commendAdded; } }

        public PullRequestCommentViewModel(IStatusIndicatorService status, IAlertDialogService alert, IJsonHttpClientService jsonClient, IApplicationService applicationService) 
            : base(status, alert, jsonClient)
        {
            _applicationService = applicationService;
        }

        protected override async System.Threading.Tasks.Task Save()
        {
            var req = new Octokit.PullRequestReviewCommentCreate(Text, null, null, 0);
            var comment = await _applicationService.GitHubClient.PullRequest.Comment.Create(RepositoryOwner, RepositoryName, Id, req);
            _commendAdded.OnNext(comment);
        }
    }
}

