using System;

namespace CodeHub.Core.Filters
{
    public class MyIssuesFilterModel
    {
		public string Labels { get; }

        public IssueFilterState FilterType { get; }

        public IssueState Open { get; }

        public bool Ascending { get; }

        public IssueSort SortType { get; }

        public MyIssuesFilterModel(IssueFilterState filterState = IssueFilterState.All, IssueState issueState = IssueState.Open, 
            IssueSort sort = IssueSort.None, string labels = null, bool ascending = false)
        {
            SortType = sort;
            Open = issueState;
            Ascending = ascending;
            FilterType = filterState;
            Labels = labels;
        }

        /// <summary>
        /// Predefined 'Open' filter
        /// </summary>
        public static MyIssuesFilterModel CreateOpenFilter()
        {
            return new MyIssuesFilterModel();
        }

        /// <summary>
        /// Predefined 'Closed' filter
        /// </summary>
        public static MyIssuesFilterModel CreateClosedFilter()
        {
            return new MyIssuesFilterModel(issueState: IssueState.Closed);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(MyIssuesFilterModel))
                return false;
            MyIssuesFilterModel other = (MyIssuesFilterModel)obj;
            return Labels == other.Labels && FilterType == other.FilterType && Open == other.Open && Ascending == other.Ascending && SortType == other.SortType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Labels != null ? Labels.GetHashCode() : 0) ^ FilterType.GetHashCode() ^ Open.GetHashCode() ^ (Ascending != null ? Ascending.GetHashCode() : 0) ^ SortType.GetHashCode();
            }
        }

        public static bool operator ==(MyIssuesFilterModel a, MyIssuesFilterModel b)
        {
            return a?.Equals(b) == true;
        }

        public static bool operator !=(MyIssuesFilterModel a, MyIssuesFilterModel b)
        {
            return a?.Equals(b) == false;
        }
    }
}
