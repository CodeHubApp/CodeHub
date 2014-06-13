using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class SelectedMilestoneMessage
	{
	    public SelectedMilestoneMessage(MilestoneModel milestone)
	    {
	        Milestone = milestone;
	    }

	    public MilestoneModel Milestone { get; private set; }
	}
}

