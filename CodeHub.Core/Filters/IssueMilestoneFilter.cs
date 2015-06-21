
namespace CodeHub.Core.Filters
{
    public class IssueMilestoneFilter
    {
        public StateType State { get; private set; }

        public Octokit.Milestone Milestone { get; private set; }

        public static IssueMilestoneFilter WithMilestone(Octokit.Milestone milestone)
        {
            return new IssueMilestoneFilter { Milestone = milestone, State = StateType.Milestone };
        }

        public static IssueMilestoneFilter WithAny()
        {
            return new IssueMilestoneFilter { State = StateType.Any };
        }

        public static IssueMilestoneFilter WithNone()
        {
            return new IssueMilestoneFilter { State = StateType.None };
        }

        public enum StateType
        {
            Any,
            None,
            Milestone
        }
    }
}

