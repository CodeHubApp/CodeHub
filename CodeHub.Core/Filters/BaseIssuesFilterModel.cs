using CodeHub.Core.ViewModels;

namespace CodeHub.Core.Filters
{
    public abstract class BaseIssuesFilterModel<T> : FilterModel<T>
    {
        public bool Ascending { get; set; }

        public Sort SortType { get; set; }

        protected BaseIssuesFilterModel()
        {
            SortType = Sort.None;
            Ascending = false;
        }

        public Octokit.IssueSort GetSort()
        {
            if (SortType == Sort.Updated)
                return Octokit.IssueSort.Updated;
            if (SortType == Sort.Comments)
                return Octokit.IssueSort.Comments;
            return Octokit.IssueSort.Created;
        }


        public enum Sort
        {
            None,
            Created,
            Updated,
            Comments
        }
    }
}

