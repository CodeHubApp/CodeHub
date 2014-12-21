using System;
using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Reactive.Subjects;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitCommentViewModel : MarkdownComposerViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Subject<CommentModel> _commendAdded = new Subject<CommentModel>();

        public string Node { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IObservable<CommentModel> CommentAdded { get { return _commendAdded; } }

        public CommitCommentViewModel(IApplicationService applicationService) 
        {
            _applicationService = applicationService;
        }

        protected override async System.Threading.Tasks.Task Save()
        {
            var comment = await _applicationService.Client.ExecuteAsync(
                _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Comments.Create(Text));
            _commendAdded.OnNext(comment.Data);
        }
    }
}

