using System;
using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Services;
using System.Reactive.Subjects;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCommentViewModel : MarkdownComposerViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Subject<GistCommentModel> _commendAdded = new Subject<GistCommentModel>();

        public string Id { get; set; }

        public IObservable<GistCommentModel> CommentAdded { get { return _commendAdded; } }

        public GistCommentViewModel(IApplicationService applicationService) 
        {
            _applicationService = applicationService;
        }

        protected override async System.Threading.Tasks.Task Save()
        {
            var comment = await _applicationService.Client.ExecuteAsync(
                _applicationService.Client.Gists[Id].CreateGistComment(Text));
            _commendAdded.OnNext(comment.Data);
        }
    }
}

