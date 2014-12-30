using System;
using ReactiveUI;
using GitHubSharp.Models;
using Humanizer;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestItemViewModel : ReactiveObject
    {
        public string Title { get; private set; }

        public GitHubAvatar Avatar { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        public string Details { get; private set; }

        internal PullRequestItemViewModel(PullRequestModel pullRequest, Action gotoAction) 
        {
            Title = pullRequest.Title ?? "No Title";
            Avatar = new GitHubAvatar(pullRequest.User.AvatarUrl);
            Details = string.Format("#{0} opened {1} by {2}", pullRequest.Number, pullRequest.CreatedAt.UtcDateTime.Humanize(), pullRequest.User.Login);
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction());
        }
    }
}

