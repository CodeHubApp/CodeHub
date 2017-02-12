using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class PullRequestEditMessage
    {
        public PullRequestModel PullRequest { get; }

        public PullRequestEditMessage(PullRequestModel pullRequest)
        {
            PullRequest = pullRequest;
        }
    }
}

