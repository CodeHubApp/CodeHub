using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using System.Reactive;
using GitHubSharp;

namespace CodeHub.Core.ViewModels.Repositories
{
    public abstract class BaseRepositoriesViewModel : BaseViewModel, IPaginatableViewModel, IProvidesSearchKeyword
    {
        protected readonly IApplicationService ApplicationService;
        private RepositoriesFilterModel _filter;

        public RepositoriesFilterModel Filter
        {
            get { return _filter; }
            private set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        public bool ShowRepositoryDescription
        {
			get { return ApplicationService.Account.ShowRepositoryDescriptionInList; }
        }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Repositories { get; private set; }

        public bool ShowRepositoryOwner { get; protected set; }

        public IReactiveCommand<Unit> LoadCommand { get; protected set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected BaseRepositoriesViewModel(IApplicationService applicationService)
        {
            ApplicationService = applicationService;
            ShowRepositoryOwner = true;
            Title = "Repositories";

            var gotoRepository = new Action<RepositoryItemViewModel>(x =>
            {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner;
                vm.RepositoryName = x.Name;
                NavigateTo(vm);
            });

            var repositories = new ReactiveList<RepositoryModel>();
            Repositories = repositories.CreateDerivedCollection(
                x => new RepositoryItemViewModel(x.Name, x.Owner.Login, x.Owner.AvatarUrl, 
                    ShowRepositoryDescription ? x.Description : string.Empty, x.StargazersCount, x.ForksCount, 
                    ShowRepositoryOwner, gotoRepository), 
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            //Filter = applicationService.Account.Filters.GetFilter<RepositoriesFilterModel>(filterKey);

            this.WhenAnyValue(x => x.Filter).Skip(1).Subscribe(_ => LoadCommand.ExecuteIfCan());

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                repositories.SimpleCollectionLoad(CreateRequest(), t as bool?, 
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x())));

//			_repositories.FilteringFunction = x => Repositories.Filter.Ascending ? x.OrderBy(y => y.Name) : x.OrderByDescending(y => y.Name);
//            _repositories.GroupingFunction = CreateGroupedItems;
        }

        protected abstract GitHubRequest<List<RepositoryModel>> CreateRequest();

//        private IEnumerable<IGrouping<string, RepositoryModel>> CreateGroupedItems(IEnumerable<RepositoryModel> model)
//        {
//            var order = Filter.OrderBy;
//            if (order == RepositoriesFilterModel.Order.Forks)
//            {
//                var a = model.OrderBy(x => x.Forks).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Forks));
//                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
//                return FilterGroup.CreateNumberedGroup(a, "Forks");
//            }
//            if (order == RepositoriesFilterModel.Order.LastUpdated)
//            {
//                var a = model.OrderByDescending(x => x.UpdatedAt).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
//                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
//                return FilterGroup.CreateNumberedGroup(a, "Days Ago", "Updated");
//            }
//            if (order == RepositoriesFilterModel.Order.CreatedOn)
//            {
//                var a = model.OrderByDescending(x => x.CreatedAt).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
//                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
//                return FilterGroup.CreateNumberedGroup(a, "Days Ago", "Created");
//            }
//            if (order == RepositoriesFilterModel.Order.Followers)
//            {
//                var a = model.OrderBy(x => x.Watchers).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Watchers));
//                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
//                return FilterGroup.CreateNumberedGroup(a, "Followers");
//            }
//            if (order == RepositoriesFilterModel.Order.Owner)
//            {
//                var a = model.OrderBy(x => x.Name).GroupBy(x => x.Owner.Login);
//                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
//                return a.ToList();
//            }
//
//            return null;
//        }
    }
}