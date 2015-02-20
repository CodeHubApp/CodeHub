using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueCommentViewModel : MarkdownComposerViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int Id { get; set; }

        public IReactiveCommand<GitHubSharp.Models.IssueCommentModel> SaveCommand { get; private set; }

        public IssueCommentViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, IAlertDialogFactory alertDialogFactory) 
            : base(imgurService, mediaPicker, alertDialogFactory)
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                async t => {
                var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].CreateComment(Text);
                return (await applicationService.Client.ExecuteAsync(request)).Data;
            });
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

