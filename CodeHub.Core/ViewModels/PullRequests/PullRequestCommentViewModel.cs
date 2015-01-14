using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommentViewModel : MarkdownComposerViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int Id { get; set; }

        public IReactiveCommand<Octokit.PullRequestReviewComment> SaveCommand { get; protected set; }

        public PullRequestCommentViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, IStatusIndicatorService statusIndicatorService) 
            : base(imgurService, mediaPicker, statusIndicatorService)
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => 
                {
                    var req = new Octokit.PullRequestReviewCommentCreate(Text, null, null, 0);
                    return applicationService.GitHubClient.PullRequest.Comment.Create(RepositoryOwner, RepositoryName, Id, req);
                });

            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

