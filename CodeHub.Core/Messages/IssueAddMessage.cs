using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class IssueAddMessage
	{
        public IssueModel Issue { get; private set; }

	    public IssueAddMessage(IssueModel issue)
	    {
	        Issue = issue;
	    }
	}
}

