namespace CodeHub.Core.Filters
{
	public abstract class BaseIssuesFilterModel
    {
		public bool Ascending { get; set; }

		public Sort SortType { get; set; }

	    protected BaseIssuesFilterModel()
        {
			SortType = Sort.None;
			Ascending = false;
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

