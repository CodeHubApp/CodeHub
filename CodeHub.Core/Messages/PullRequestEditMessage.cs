namespace CodeHub.Core.Messages
{
    public class PullRequestEditMessage
    {
        public Octokit.PullRequest PullRequest { get; }

        public PullRequestEditMessage(Octokit.PullRequest pullRequest)
        {
            PullRequest = pullRequest;
        }
    }
}

