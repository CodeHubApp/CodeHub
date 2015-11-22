using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using Humanizer;

namespace CodeHub.Core.ViewModels.Search
{
    public class ExploreViewModel : BaseViewModel
    {
        public RepositoryExploreViewModel Repositories { get; }

        public UserExploreViewModel Users { get; }

        private SearchType _searchFilter;
        public SearchType SearchFilter
        {
            get { return _searchFilter; }
            set { this.RaiseAndSetIfChanged(ref _searchFilter, value); }
        }
  
        public IReactiveCommand<object> SearchCommand { get; private set; }

        public ExploreViewModel(ISessionService applicationService)
        {
            Title = "Explore";
            Repositories = new RepositoryExploreViewModel(applicationService);
            Users = new UserExploreViewModel(applicationService);
            SearchCommand = ReactiveCommand.Create();
        }

        public enum SearchType
        {
            Repositories = 0,
            Users
        }
    }
}

