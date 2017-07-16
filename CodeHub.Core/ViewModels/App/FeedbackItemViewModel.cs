using System;
using ReactiveUI;
using Humanizer;
using System.Reactive;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Title { get; }

        public string ImageUrl { get; }

        public ReactiveCommand<Unit, Unit> GoToCommand { get; } = ReactiveCommand.Create(() => { });

        public DateTimeOffset Created { get; }

        public string CreatedString { get; }

        public string RepositoryName { get; }

        public string RepositoryOwner { get; }

        public int IssueId { get; }

        internal FeedbackItemViewModel(string repositoryOwner, string repositoryName, Octokit.Issue issue)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            IssueId = issue.Number;
            Title = issue.Title;
            ImageUrl = issue.User.AvatarUrl;
            Created = issue.CreatedAt;
            CreatedString = Created.Humanize();
        }
    }
}

