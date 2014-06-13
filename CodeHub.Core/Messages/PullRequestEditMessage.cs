using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class PullRequestEditMessage
    {
        public PullRequestEditMessage(PullRequestModel pullRequest)
        {
            PullRequest = pullRequest;
        }

        public PullRequestModel PullRequest { get; private set; }
    }
}

