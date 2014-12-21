using System;

namespace CodeHub.Core.Filters
{
	public class MyIssuesFilterModel : BaseIssuesFilterModel
    {
		public string Labels { get; set; }

		public Filter FilterType { get; set; }

        public bool Open { get; set; }

		public DateTime? Since { get; set; }

		public MyIssuesFilterModel()
        {
            Open = true;
            FilterType = Filter.All;
        }

        /// <summary>
        /// Predefined 'Open' filter
        /// </summary>
        public static MyIssuesFilterModel CreateOpenFilter()
        {
            return new MyIssuesFilterModel { FilterType = Filter.All, Open = true };
        }

        /// <summary>
        /// Predefined 'Closed' filter
        /// </summary>
        public static MyIssuesFilterModel CreateClosedFilter()
        {
            return new MyIssuesFilterModel { FilterType = Filter.All, Open = false };
        }

        public MyIssuesFilterModel Clone()
        {
            return (MyIssuesFilterModel)MemberwiseClone();
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
            return Ascending == other.Ascending && Labels == other.Labels && FilterType == other.FilterType && Open == other.Open && Since == other.Since && SortType == other.SortType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Ascending.GetHashCode() ^ (Labels != null ? Labels.GetHashCode() : 0) ^ FilterType.GetHashCode() ^ Open.GetHashCode() ^ Since.GetHashCode() ^ SortType.GetHashCode();
            }
        }
        
		public enum Filter
		{
            //[EnumDescription("Assigned To You")]
			Assigned,
            //[EnumDescription("Created By You")]
			Created,
            //[EnumDescription("Mentioning You")]
			Mentioned,
            //[EnumDescription("Issues Subscribed To")]
			Subscribed,
			All
		}
    }
}

