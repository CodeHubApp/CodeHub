namespace CodeHub.Core.Filters
{
	public class MyIssuesFilterModel
    {
		public string Labels { get; set; }

        public IssueFilterState FilterType { get; set; }

        public IssueState Open { get; set; }

        public bool Ascending { get; set; }

        public IssueSort SortType { get; set; }

		public MyIssuesFilterModel()
        {
            SortType = IssueSort.None;
            Open = IssueState.Open;
            Ascending = false;
            FilterType = IssueFilterState.All;
        }

        /// <summary>
        /// Predefined 'Open' filter
        /// </summary>
        public static MyIssuesFilterModel CreateOpenFilter()
        {
            return new MyIssuesFilterModel { FilterType = IssueFilterState.All, Open = IssueState.Open };
        }

        /// <summary>
        /// Predefined 'Closed' filter
        /// </summary>
        public static MyIssuesFilterModel CreateClosedFilter()
        {
            return new MyIssuesFilterModel { FilterType = IssueFilterState.All, Open = IssueState.Closed };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(MyIssuesFilterModel))
                return false;
            var other = (MyIssuesFilterModel)obj;
            return Ascending == other.Ascending && Labels == other.Labels && FilterType == other.FilterType && Open == other.Open && SortType == other.SortType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Ascending.GetHashCode() ^ (Labels != null ? Labels.GetHashCode() : 0) ^ FilterType.GetHashCode() ^ Open.GetHashCode() ^ SortType.GetHashCode();
            }
        }
        

    }
}

