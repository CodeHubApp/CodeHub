using System;
using CodeFramework.Core.Utils;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class MyIssuesViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly FilterableCollectionViewModel<IssueModel, MyIssuesFilterModel> _issues;
        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            protected set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }

        public FilterableCollectionViewModel<IssueModel, MyIssuesFilterModel> Issues
        {
            get { return _issues; }
        }

        public MyIssuesViewModel()
        {
            _issues = new FilterableCollectionViewModel<IssueModel, MyIssuesFilterModel>("MyIssues");
            _issues.GroupingFunction = Group;
            _issues.Bind(x => x.Filter, async () =>
            {
                try
                {
                    IsLoading = true;
                    await Load(true);
                }
                catch (Exception e)
                {
                    //Do nothing...
                }
                finally
                {
                    IsLoading = false;
                }
            });
           
        }

        public Task Load(bool forceDataRefresh)
        {
            string filter = Issues.Filter.FilterType.ToString().ToLower();
            string direction = Issues.Filter.Ascending ? "asc" : "desc";
            string state = Issues.Filter.Open ? "open" : "closed";
            string sort = Issues.Filter.SortType == MyIssuesFilterModel.Sort.None ? null : Issues.Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(Issues.Filter.Labels) ? null : Issues.Filter.Labels;

            var request = Application.Client.AuthenticatedUser.Issues.GetAll(sort: sort, labels: labels, state: state, direction: direction, filter: filter);
            return Issues.SimpleCollectionLoad(request, forceDataRefresh);
        }
        
        private List<IGrouping<string, IssueModel>> Group(IEnumerable<IssueModel> model)
        {
            var order = Issues.Filter.SortType;
            if (order == MyIssuesFilterModel.Sort.Comments)
            {
                var a = Issues.Filter.Ascending ? model.OrderBy(x => x.Comments) : model.OrderByDescending(x => x.Comments);
                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Comments)).ToList();
                return FilterGroup.CreateNumberedGroup(g, "Comments");
            }
            else if (order == MyIssuesFilterModel.Sort.Updated)
            {
                var a = Issues.Filter.Ascending ? model.OrderBy(x => x.UpdatedAt) : model.OrderByDescending(x => x.UpdatedAt);
                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
                return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Updated");
            }
            else if (order == MyIssuesFilterModel.Sort.Created)
            {
                var a = Issues.Filter.Ascending ? model.OrderBy(x => x.CreatedAt) : model.OrderByDescending(x => x.CreatedAt);
                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
                return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Created");
            }

            return null;
        }
    }
}

