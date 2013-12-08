using CodeFramework.Core.ViewModels;

namespace CodeHub.Core.Filters
{
	public abstract class BaseIssuesFilterModel<T> : FilterModel<T>
    {
		public bool Ascending { get; set; }

		public Sort SortType { get; set; }

        public BaseIssuesFilterModel()
        {
			SortType = Sort.None;
			Ascending = false;
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

