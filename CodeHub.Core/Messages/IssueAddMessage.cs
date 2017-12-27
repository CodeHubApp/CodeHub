using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class IssueAddMessage
    {
        public Octokit.Issue Issue { get; }

        public IssueAddMessage(Octokit.Issue issue)
        {
            Issue = issue;
        }
    }
}

