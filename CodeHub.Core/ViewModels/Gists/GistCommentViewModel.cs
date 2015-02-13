using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCommentViewModel : MarkdownComposerViewModel
    {
        public string Id { get; set; }

        public IReactiveCommand<GitHubSharp.Models.GistCommentModel> SaveCommand { get; protected set; }

        public GistCommentViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, IAlertDialogFactory alertDialogFactory) 
            : base(imgurService, mediaPicker, alertDialogFactory)
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                async t => 
                {
                    var request = applicationService.Client.Gists[Id].CreateGistComment(Text);
                    return (await applicationService.Client.ExecuteAsync(request)).Data;
                });
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

