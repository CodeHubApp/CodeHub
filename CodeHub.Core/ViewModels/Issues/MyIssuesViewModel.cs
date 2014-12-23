using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public class MyIssuesViewModel : BaseIssuesViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly MyIssuesFilterModel _openFilter = MyIssuesFilterModel.CreateOpenFilter();
        private readonly MyIssuesFilterModel _closedFilter = MyIssuesFilterModel.CreateClosedFilter();

        private readonly ObservableAsPropertyHelper<int> _selectedFilter;
		public int SelectedFilter
		{
            get { return _selectedFilter.Value; }
			set
            {
                switch (value)
                {
                    case 0:
                        Filter = _openFilter;
                        break;
                    case 1:
                        Filter = _closedFilter;
                        break;
                }
            }
		}

        private MyIssuesFilterModel _filter;
        private MyIssuesFilterModel Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        public MyIssuesViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            Title = "My Issues";
            Filter = MyIssuesFilterModel.CreateOpenFilter();

            _selectedFilter = this.WhenAnyValue(x => x.Filter)
                .Select(x =>
                {
                    if (x == null || _openFilter.Equals(x))
                        return 0;
                    return _closedFilter.Equals(x) ? 1 : -1;
                })
                .ToProperty(this, x => x.SelectedFilter);

            this.WhenAnyValue(x => x.Filter).Skip(1).Subscribe(_ =>
            {
                IssuesBacking.Clear();
                LoadCommand.ExecuteIfCan();
            });
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.IssueModel>> CreateRequest()
        {
            var filter = Filter.FilterType.ToString().ToLower();
            var direction = Filter.Ascending ? "asc" : "desc";
            var state = Filter.Open ? "open" : "closed";
            var sort = Filter.SortType == BaseIssuesFilterModel.Sort.None
                ? null : Filter.SortType.ToString().ToLower();
            var labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;
            return _applicationService.Client.AuthenticatedUser.Issues.GetAll(sort: sort, labels: labels,
                state: state, direction: direction, filter: filter);
        }
    }
}

