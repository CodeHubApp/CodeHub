using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class SelectedAssignedToMessage
	{
	    public SelectedAssignedToMessage(BasicUserModel user)
	    {
	        User = user;
	    }

	    public BasicUserModel User { get; private set; }
	}
}

