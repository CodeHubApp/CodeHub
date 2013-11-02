using CodeFramework.Core.Utils;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public abstract class RepositoriesViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly FilterableCollectionViewModel<RepositoryModel, RepositoriesFilterModel> _repositories;

        public FilterableCollectionViewModel<RepositoryModel, RepositoriesFilterModel> Repositories
        {
            get { return _repositories; }
        }

        protected RepositoriesViewModel(string filterKey = "RepositoryController")
        {
            _repositories = new FilterableCollectionViewModel<RepositoryModel, RepositoriesFilterModel>(filterKey);
            _repositories.FilteringFunction = x => _repositories.Filter.Ascending ? x.OrderBy(y => y.Name) : x.OrderByDescending(y => y.Name);
            _repositories.GroupingFunction = CreateGroupedItems;
            _repositories.Bind(x => x.Filter, () => Repositories.Refresh());
        }

        private IEnumerable<IGrouping<string, RepositoryModel>> CreateGroupedItems(IEnumerable<RepositoryModel> model)
        {
            var order = (RepositoriesFilterModel.Order)Repositories.Filter.OrderBy;
            if (order == RepositoriesFilterModel.Order.Forks)
            {
                var a = model.OrderBy(x => x.Forks).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Forks));
                a = Repositories.Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return FilterGroup.CreateNumberedGroup(a, "Forks");
            }
            if (order == RepositoriesFilterModel.Order.LastUpdated)
            {
                var a = model.OrderByDescending(x => x.UpdatedAt).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
                a = Repositories.Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return FilterGroup.CreateNumberedGroup(a, "Days Ago", "Updated");
            }
            if (order == RepositoriesFilterModel.Order.CreatedOn)
            {
                var a = model.OrderByDescending(x => x.CreatedAt).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
                a = Repositories.Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return FilterGroup.CreateNumberedGroup(a, "Days Ago", "Created");
            }
            if (order == RepositoriesFilterModel.Order.Followers)
            {
                var a = model.OrderBy(x => x.Watchers).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Watchers));
                a = Repositories.Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return FilterGroup.CreateNumberedGroup(a, "Followers");
            }
            if (order == RepositoriesFilterModel.Order.Owner)
            {
                var a = model.OrderBy(x => x.Name).GroupBy(x => x.Owner.Login);
                a = Repositories.Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return a.ToList();
            }

            return null;
        }

        public abstract Task Load(bool forceDataRefresh);
    }
}