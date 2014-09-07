using System;
using ReactiveUI;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestItemViewModel : ReactiveObject
    {
        public string Title { get; private set; }

        public string ImageUrl { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        public PullRequestModel PullRequest { get; private set; }

        public string Details { get; private set; }

        internal PullRequestItemViewModel(PullRequestModel pullRequest, IReactiveCommand action) 
        {
            PullRequest = pullRequest;
            Title = pullRequest.Title ?? "No Title";
            ImageUrl = pullRequest.User.AvatarUrl;
            Details = string.Format("#{0} opened {1} by {2}", pullRequest.Number, pullRequest.CreatedAt.ToDaysAgo(), pullRequest.User.Login);
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => action.ExecuteIfCan(this));
        }
    }
}

