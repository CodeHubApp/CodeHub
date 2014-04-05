using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CodeHub.Core.ViewModels.Issues
{
	public class MyIssuesViewModel : BaseIssuesViewModel<MyIssuesFilterModel>
    {
		private int _selectedFilter;

		public int SelectedFilter
		{
			get { return _selectedFilter; }
			set 
			{
				_selectedFilter = value;
				RaisePropertyChanged(() => SelectedFilter);
			}
		}

        public MyIssuesViewModel()
        {
            _issues = new FilterableCollectionViewModel<IssueModel, MyIssuesFilterModel>("MyIssues");
            _issues.GroupingFunction = Group;
            _issues.Bind(x => x.Filter, () => LoadCommand.Execute(false));

			this.Bind(x => x.SelectedFilter, x =>
			{
				if (x == 0)
					_issues.Filter = MyIssuesFilterModel.CreateOpenFilter();
				else if (x == 1)
					_issues.Filter = MyIssuesFilterModel.CreateClosedFilter();
			});
        }

        protected override List<IGrouping<string, IssueModel>> Group(IEnumerable<IssueModel> model)
        {
            var group = base.Group(model);
            if (group == null)
            {
                try
                {
                    var regex = new System.Text.RegularExpressions.Regex("repos/(.+)/issues/");
                    return model.GroupBy(x => regex.Match(x.Url).Groups[1].Value).ToList();
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            return group;
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
            string filter = Issues.Filter.FilterType.ToString().ToLower();
            string direction = Issues.Filter.Ascending ? "asc" : "desc";
            string state = Issues.Filter.Open ? "open" : "closed";
            string sort = Issues.Filter.SortType == MyIssuesFilterModel.Sort.None ? null : Issues.Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(Issues.Filter.Labels) ? null : Issues.Filter.Labels;

			var request = this.GetApplication().Client.AuthenticatedUser.Issues.GetAll(sort: sort, labels: labels, state: state, direction: direction, filter: filter);
            return Issues.SimpleCollectionLoad(request, forceCacheInvalidation);
        }
    }
}

