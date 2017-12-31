using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class IssueEditMessage
    {
        public IssueModel Issue { get; }

        public IssueEditMessage(IssueModel issue)
        {
            Issue = issue;
        }
    }
}

