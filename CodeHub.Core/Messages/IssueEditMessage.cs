using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class IssueEditMessage
	{
	    public IssueEditMessage(IssueModel issue)
	    {
	        Issue = issue;
	    }

	    public IssueModel Issue { get; private set; }
	}
}

