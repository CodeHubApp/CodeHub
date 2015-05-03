using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommentViewModel : BaseViewModel, IComposerViewModel
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public int PullRequestId { get; private set; }

        public string Path { get; private set; }

        public string CommitSha { get; private set; }

        public int Position { get; private set; }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public IReactiveCommand<Octokit.PullRequestReviewComment> SaveCommand { get; private set; }

        public IReactiveCommand<bool> DismissCommand { get; private set; }

        public PullRequestCommentViewModel(ISessionService applicationService, IAlertDialogFactory alertDialogFactory) 
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => {
                    var req = new Octokit.PullRequestReviewCommentCreate(Text, CommitSha, Path, Position);
                    return applicationService.GitHubClient.PullRequest.Comment.Create(RepositoryOwner, RepositoryName, PullRequestId, req);
                });

            SaveCommand.AlertExecuting(alertDialogFactory, "Saving...");
            SaveCommand.Subscribe(x => Dismiss());

            DismissCommand = ReactiveCommand.CreateAsyncTask(async t =>
                {
                    if (string.IsNullOrEmpty(Text))
                        return true;
                    return await alertDialogFactory.PromptYesNo("Discard Comment?", "Are you sure you want to discard this comment?");
                });
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
        }

        public PullRequestCommentViewModel Init(string repositoryOwner, string repositoryName, 
            int pullRequestId, string commitSha, string path, int position)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            PullRequestId = pullRequestId;
            Path = path;
            CommitSha = commitSha;
            Position = position;
            return this;
        }
    }
}

