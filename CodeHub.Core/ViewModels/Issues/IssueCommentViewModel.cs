using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using Xamarin.Utilities.Factories;
using Xamarin.Utilities.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueCommentViewModel : MarkdownComposerViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int Id { get; set; }

        public IReactiveCommand<Octokit.IssueComment> SaveCommand { get; private set; }

        public IssueCommentViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, IStatusIndicatorService statusIndicatorService) 
            : base(imgurService, mediaPicker, statusIndicatorService)
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => applicationService.GitHubClient.Issue.Comment.Create(RepositoryOwner, RepositoryName, Id, Text));
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

