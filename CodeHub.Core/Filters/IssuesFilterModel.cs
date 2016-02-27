using System;

namespace CodeHub.Core.Filters
{
    public class IssuesFilterModel : BaseIssuesFilterModel<IssuesFilterModel>
    {
        public string Labels { get; set; }

        public bool Open { get; set; }

        public DateTime? Since { get; set; }

        public string Mentioned { get; set; }

        public string Creator { get; set; }

        public string Assignee { get; set; }

        public MilestoneKeyValue Milestone { get; set; }

        public class MilestoneKeyValue
        {
            public string Name { get; set; }
            public bool IsMilestone { get; set; }
            public string Value { get; set; }
        }

        public IssuesFilterModel()
        {
            Ascending = false;
            Open = true;
        }

        /// <summary>
        /// Predefined 'Open' filter
        /// </summary>
        public static IssuesFilterModel CreateOpenFilter()
        {
            return new IssuesFilterModel { Open = true };
        }

        /// <summary>
        /// Predefined 'Closed' filter
        /// </summary>
        public static IssuesFilterModel CreateClosedFilter()
        {
            return new IssuesFilterModel { Open = false };
        }

        /// <summary>
        /// Predefined 'Mine' filter
        /// </summary>
        /// <returns>The mine filter.</returns>
        /// <param name="username">Username.</param>
        public static IssuesFilterModel CreateMineFilter(string username)
        {
            return new IssuesFilterModel { Assignee = username, Open = true };
        }

        public override IssuesFilterModel Clone()
        {
            return (IssuesFilterModel)this.MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(IssuesFilterModel))
                return false;
            var other = (IssuesFilterModel)obj;
            return Ascending == other.Ascending && Labels == other.Labels && Open == other.Open && Since == other.Since && SortType == other.SortType && Mentioned == other.Mentioned && Creator == other.Creator && Assignee == other.Assignee && Milestone == other.Milestone;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Ascending.GetHashCode() ^ (Labels != null ? Labels.GetHashCode() : 0) ^ Open.GetHashCode() ^ Since.GetHashCode() ^ SortType.GetHashCode() ^ (Mentioned != null ? Mentioned.GetHashCode() : 0) ^ (Creator != null ? Creator.GetHashCode() : 0) ^ (Assignee != null ? Assignee.GetHashCode() : 0) ^ (Milestone != null ? Milestone.GetHashCode() : 0);
            }
        }
    }
}

