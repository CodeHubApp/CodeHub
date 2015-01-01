using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using Xamarin.Utilities.Services;
using Xamarin.Utilities.Factories;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCommentViewModel : MarkdownComposerViewModel
    {
        public int Id { get; set; }

        public IReactiveCommand<Octokit.GistComment> SaveCommand { get; protected set; }

        public GistCommentViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, IStatusIndicatorService statusIndicatorService) 
            : base(imgurService, mediaPicker, statusIndicatorService)
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => applicationService.GitHubClient.Gist.Comment.Create(Id, Text));
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

