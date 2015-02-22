using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommentViewModel : BaseViewModel, IComposerViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int Id { get; set; }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public IReactiveCommand<Octokit.PullRequestReviewComment> SaveCommand { get; protected set; }

        public PullRequestCommentViewModel(IApplicationService applicationService) 
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

