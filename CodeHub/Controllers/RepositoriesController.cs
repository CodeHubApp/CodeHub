using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using CodeFramework.Controllers;
using CodeHub.Filters.Models;


namespace CodeHub.Controllers
{
    public class RepositoriesController : ListController<RepositoryModel, RepositoriesFilterModel>
    {
        public string Username { get; private set; }
        private readonly string _filterKey;

        public RepositoriesController(IListView<RepositoryModel> view, string username, string filterKey = "RepositoryController")
            : base(view)
        {
            Username = username;
            _filterKey = filterKey;

            Filter = Application.Account.GetFilter<RepositoriesFilterModel>(_filterKey);
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[Username].Repositories.GetAll(force);
            Model = new ListModel<RepositoryModel> {Data = response.Data, More = this.CreateMore(response)};
        }

        protected override List<RepositoryModel> FilterModel(List<RepositoryModel> model, RepositoriesFilterModel filter)
        {
            return (Filter.Ascending ? model.OrderBy(x => x.Name) : model.OrderByDescending(x => x.Name)).ToList();
        }

        protected override List<IGrouping<string, RepositoryModel>> GroupModel(List<RepositoryModel> model, RepositoriesFilterModel filter)
        {
            var order = (RepositoriesFilterModel.Order)Filter.OrderBy;
            if (order == RepositoriesFilterModel.Order.Forks)
            {
                var a = model.OrderBy(x => x.Forks).GroupBy(x => IntegerCeilings.First(r => r > x.Forks));
                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return CreateNumberedGroup(a, "Forks");
            }
            if (order == RepositoriesFilterModel.Order.LastUpdated)
            {
                var a = model.OrderByDescending(x => x.UpdatedAt).GroupBy(x => IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return CreateNumberedGroup(a, "Days Ago", "Updated");
            }
            if (order == RepositoriesFilterModel.Order.CreatedOn)
            {
                var a = model.OrderByDescending(x => x.CreatedAt).GroupBy(x => IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return CreateNumberedGroup(a, "Days Ago", "Created");
            }
            if (order == RepositoriesFilterModel.Order.Followers)
            {
                var a = model.OrderBy(x => x.Watchers).GroupBy(x => IntegerCeilings.First(r => r > x.Watchers));
                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return CreateNumberedGroup(a, "Followers");
            }
            if (order == RepositoriesFilterModel.Order.Owner)
            {
                var a = model.OrderBy(x => x.Name).GroupBy(x => x.Owner.Login);
                a = Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return a.ToList();
            }

            return null;
        }

        protected override void SaveFilterAsDefault(RepositoriesFilterModel filter)
        {
            Application.Account.AddFilter(_filterKey, filter);
        }
    }
}