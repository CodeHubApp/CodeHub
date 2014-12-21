using System;
using ReactiveUI;
using GitHubSharp.Models;
using Humanizer;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestItemViewModel : ReactiveObject
    {
        public string Title { get; private set; }

        public string ImageUrl { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        public string Details { get; private set; }

        internal PullRequestItemViewModel(PullRequestModel pullRequest, Action gotoAction) 
        {
            Title = pullRequest.Title ?? "No Title";
            ImageUrl = pullRequest.User.AvatarUrl;
            Details = string.Format("#{0} opened {1} by {2}", pullRequest.Number, pullRequest.CreatedAt.UtcDateTime.Humanize(), pullRequest.User.Login);
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction());
        }
    }
}

