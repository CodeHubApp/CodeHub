using ReactiveUI;

namespace CodeHub.Core.ViewModels.Search
{
    public class ExploreViewModel : ReactiveObject
    {
        public RepositoryExploreViewModel Repositories { get; } = new RepositoryExploreViewModel();

        public UserExploreViewModel Users { get; } = new UserExploreViewModel();

        private SearchType _searchFilter = SearchType.Repositories;
        public SearchType SearchFilter
        {
            get { return _searchFilter; }
            set { this.RaiseAndSetIfChanged(ref _searchFilter, value); }
        }
  
        public enum SearchType
        {
            Repositories = 0,
            Users
        }
    }
}

