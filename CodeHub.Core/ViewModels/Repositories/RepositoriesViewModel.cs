using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Utils;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Repositories
{
    public abstract class RepositoriesViewModel : LoadableViewModel
    {
        private readonly FilterableCollectionViewModel<RepositoryModel, RepositoriesFilterModel> _repositories;

        public bool ShowRepositoryDescription
        {
			get { return this.GetApplication().Account.ShowRepositoryDescriptionInList; }
        }

        public FilterableCollectionViewModel<RepositoryModel, RepositoriesFilterModel> Repositories
        {
            get { return _repositories; }
        }

        public bool ShowRepositoryOwner { get; protected set; }

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand<RepositoryModel>(x => this.ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner.Login, Repository = x.Name })); }
        }

        protected RepositoriesViewModel(string filterKey = "RepositoryController")
        {
            _repositories = new FilterableCollectionViewModel<RepositoryModel, RepositoriesFilterModel>(filterKey);
			_repositories.FilteringFunction = x => Repositories.Filter.Ascending ? x.OrderBy(y => y.Name) : x.OrderByDescending(y => y.Name);
            _repositories.GroupingFunction = CreateGroupedItems;
            _repositories.Bind(x => x.Filter).Subscribe(x => Repositories.Refresh());
        }

        private IEnumerable<IGrouping<string, RepositoryModel>> CreateGroupedItems(IEnumerable<RepositoryModel> model)
        {
            var order = Repositories.Filter.OrderBy;
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
    }
}