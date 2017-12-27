namespace CodeHub.Core.Messages
{
    public class IssueEditMessage
    {
        public Octokit.Issue Issue { get; }

        public IssueEditMessage(Octokit.Issue issue)
        {
            Issue = issue;
        }
    }
}

