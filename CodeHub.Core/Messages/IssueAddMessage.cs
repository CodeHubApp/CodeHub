using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class IssueAddMessage
    {
        public IssueModel Issue { get; }

        public IssueAddMessage(IssueModel issue)
        {
            Issue = issue;
        }
    }
}

