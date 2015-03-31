using CodeHub.Core.Utilities;

namespace CodeHub.Core.Filters
{
    public enum IssueFilterState
    {
        [EnumDescription("Assigned To You")]
        Assigned,
        [EnumDescription("Created By You")]
        Created,
        [EnumDescription("Mentioning You")]
        Mentioned,
        [EnumDescription("Issues Subscribed To")]
        Subscribed,
        All
    }
}

