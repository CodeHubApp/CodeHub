using Octokit;
using System.Collections.Generic;

namespace CodeHub.Core.Filters
{
    public class IssuesFilterModel
    {
        public User Assignee { get; }

        public string Creator { get; }

        public string Mentioned { get; }

        public IReadOnlyList<Label> Labels { get; }

        public Milestone Milestone { get; }

        public IssueState IssueState { get; }

        public bool Ascending { get; }

        public IssueSort SortType { get; }

        public IssuesFilterModel(User assignee = null, string creator = null, string mentioned = null, IReadOnlyList<Label> labels = null,
            Milestone milestone = null, IssueState issueState = IssueState.Open, 
            IssueSort sortType = IssueSort.None, bool ascending = false)
        {
            Assignee = assignee;
            Creator = creator;
            Mentioned = mentioned;
            Labels = labels;
            this.Milestone = milestone;
            this.IssueState = issueState;
            SortType = sortType;
            Ascending = ascending;
        }

        public static IssuesFilterModel CreateOpenFilter()
        {
            return new IssuesFilterModel();
        }

        public static IssuesFilterModel CreateClosedFilter()
        {
            return new IssuesFilterModel(issueState: IssueState.Closed);
        }

        public static IssuesFilterModel CreateMineFilter(User user)
        {
            return new IssuesFilterModel(assignee: user);
        }
    }
}

