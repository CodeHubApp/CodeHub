using System;
using ReactiveUI;
using Humanizer;
using CodeHub.Core.Utilities;
using Octokit;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Title { get; private set; }

        public GitHubAvatar Avatar { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        public string Details { get; private set; }

        internal PullRequest PullRequest { get; private set; }

        internal PullRequestItemViewModel(PullRequest pullRequest) 
        {
            PullRequest = pullRequest;

            var login = pullRequest?.User.Login ?? "Unknonwn User";
            var avatar = pullRequest?.User.AvatarUrl;
            Title = pullRequest.Title ?? "No Title";
            Avatar = new GitHubAvatar(avatar);
            Details = string.Format("#{0} opened {1} by {2}", pullRequest.Number, pullRequest.CreatedAt.UtcDateTime.Humanize(), login);
            GoToCommand = ReactiveCommand.Create();
        }
    }
}

