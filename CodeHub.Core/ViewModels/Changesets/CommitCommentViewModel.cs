using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitCommentViewModel : MarkdownComposerViewModel
    {
        public string Node { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IReactiveCommand<GitHubSharp.Models.CommentModel> SaveCommand { get; protected set; }

        public CommitCommentViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, IAlertDialogFactory alertDialogFactory) 
            : base(imgurService, mediaPicker, alertDialogFactory)
        {
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                async t => 
                {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Comments.Create(Text);
                    return (await applicationService.Client.ExecuteAsync(request)).Data;
                });
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

