using System;
using CodeFramework.Filters.Models;

namespace CodeHub.Filters.Models
{
    public class IssuesFilterModel : FilterModel<IssuesFilterModel>
    {
        public bool Ascending { get; set; }

        public string Labels { get; set; }

        public bool Open { get; set; }

        public DateTime? Since { get; set; }

        public Sort SortType { get; set; }

        public string Mentioned { get; set; }

        public string Creator { get; set; }

        public string Assignee { get; set; }

        public string Milestone { get; set; }


        public IssuesFilterModel()
        {
            Ascending = false;
            Open = true;
            SortType = Sort.None;
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
        /// Predefined 'All' filter
        /// </summary>
        /// <returns>The all filter.</returns>
        public static IssuesFilterModel CreateAllFilter()
        {
            return new IssuesFilterModel();
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
            IssuesFilterModel other = (IssuesFilterModel)obj;
            return Ascending == other.Ascending && Labels == other.Labels && Open == other.Open && Since == other.Since && SortType == other.SortType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Ascending.GetHashCode() ^ (Labels != null ? Labels.GetHashCode() : 0) ^ Open.GetHashCode() ^ Since.GetHashCode() ^ SortType.GetHashCode();
            }
        }

        public enum Sort : int
        {
            None,
            Created,
            Updated,
            Comments
        }
    }
}

