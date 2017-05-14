using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class SelectedMilestoneMessage
    {
        public MilestoneModel Milestone { get; }

        public SelectedMilestoneMessage(MilestoneModel milestone)
        {
            Milestone = milestone;
        }
    }
}

