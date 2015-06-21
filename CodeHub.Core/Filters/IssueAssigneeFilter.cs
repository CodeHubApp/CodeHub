
namespace CodeHub.Core.Filters
{
    public class IssueAssigneeFilter
    {
        public bool NoAssignedUser { get; private set; }

        public string AssignedUser { get; private set; }

        public static IssueAssigneeFilter WithUser(string assignedUser)
        {
            return new IssueAssigneeFilter { AssignedUser = assignedUser };
        }

        public static IssueAssigneeFilter WithNoUser()
        {
            return new IssueAssigneeFilter { NoAssignedUser = true };
        }
    }
}

