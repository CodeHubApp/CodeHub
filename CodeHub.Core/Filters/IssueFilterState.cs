using CodeHub.Core.Utilities;

namespace CodeHub.Core.Filters
{
    public enum IssueFilterState
    {
        [Description("Assigned To You")]
        Assigned,
        [Description("Created By You")]
        Created,
        [Description("Mentioning You")]
        Mentioned,
        [Description("Issues Subscribed To")]
        Subscribed,
        All
    }
}

