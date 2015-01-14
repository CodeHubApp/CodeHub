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

        public IReactiveCommand<Octokit.CommitComment> SaveCommand { get; protected set; }

        public CommitCommentViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, IStatusIndicatorService statusIndicatorService) 
            : base(imgurService, mediaPicker, statusIndicatorService)
        {
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => 
                {
                    var commitComment = new Octokit.NewCommitComment(Text);
                    return applicationService.GitHubClient.Repository.RepositoryComments.Create(RepositoryOwner, RepositoryName, Node, commitComment);
                });
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

