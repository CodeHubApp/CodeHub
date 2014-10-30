using System;
using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Services;
using System.Reactive.Subjects;
using GitHubSharp.Models;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCommentViewModel : MarkdownComposerViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Subject<GistCommentModel> _commendAdded = new Subject<GistCommentModel>();

        public string Id { get; set; }

        public IObservable<GistCommentModel> CommentAdded { get { return _commendAdded; } }

        public GistCommentViewModel(IStatusIndicatorService status, IAlertDialogService alert, IJsonHttpClientService jsonClient, IApplicationService applicationService) 
            : base(status, alert, jsonClient)
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

