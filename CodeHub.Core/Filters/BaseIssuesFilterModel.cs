namespace CodeHub.Core.Filters
{
	public abstract class BaseIssuesFilterModel
    {
		public bool Ascending { get; set; }

		public IssueSort SortType { get; set; }

	    protected BaseIssuesFilterModel()
        {
            SortType = IssueSort.None;
			Ascending = false;
        }
    }
}

