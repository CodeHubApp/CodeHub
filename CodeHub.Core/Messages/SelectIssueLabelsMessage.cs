using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class SelectIssueLabelsMessage
    {
	    public SelectIssueLabelsMessage(LabelModel[] labels)
	    {
	        Labels = labels;
	    }

	    public GitHubSharp.Models.LabelModel[] Labels { get; private set; }
    }
}

