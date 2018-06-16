namespace CodeHub.Core.Messages
{
    public class SelectedMilestoneMessage
    {
        public Octokit.Milestone Milestone { get; }

        public SelectedMilestoneMessage(Octokit.Milestone milestone)
        {
            Milestone = milestone;
        }
    }
}

