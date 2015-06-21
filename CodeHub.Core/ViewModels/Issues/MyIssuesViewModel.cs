using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public class MyIssuesViewModel : BaseIssuesViewModel
    {
        private readonly ISessionService _applicationService;
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
                        CustomFilterEnabled = false;
                        break;
                    case 1:
                        Filter = _closedFilter;
                        CustomFilterEnabled = false;
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

        private bool _customFilterEnabled;
        public bool CustomFilterEnabled
        {
            get { return _customFilterEnabled; }
            set { this.RaiseAndSetIfChanged(ref _customFilterEnabled, value); }
        }

        public IReactiveCommand<object> GoToFilterCommand { get; private set; }

        public MyIssuesViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            _applicationService = sessionService;

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
 
            GoToFilterCommand = ReactiveCommand.Create();
            GoToFilterCommand.Subscribe(_ => {
                var vm = this.CreateViewModel<MyIssuesFilterViewModel>();
                vm.Ascending = Filter.Ascending;
                vm.FilterType = Filter.FilterType;
                vm.Labels = Filter.Labels;
                vm.State = Filter.Open;
                vm.SortType = Filter.SortType;
                vm.SaveCommand.Subscribe(__ => {
                    Filter = new MyIssuesFilterModel 
                    {
                        Ascending = vm.Ascending,
                        FilterType = vm.FilterType,
                        Labels = vm.Labels,
                        Open = vm.State,
                        SortType = vm.SortType
                    };
                    CustomFilterEnabled = true;
                });

                NavigateTo(vm);
            });
        }

        protected override bool IssueFilter(GitHubSharp.Models.IssueModel issue)
        {
            if (Filter == null)
                return base.IssueFilter(issue);

            if (Filter.Open == IssueState.Open)
                return base.IssueFilter(issue) && string.Equals(issue.State, "open", StringComparison.OrdinalIgnoreCase);
            if (Filter.Open == IssueState.Closed)
                return base.IssueFilter(issue) && string.Equals(issue.State, "closed", StringComparison.OrdinalIgnoreCase);
            return base.IssueFilter(issue);
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.IssueModel>> CreateRequest()
        {
            var filter = Filter.FilterType.ToString().ToLower();
            var direction = Filter.Ascending ? "asc" : "desc";
            var state = Filter.Open.ToString().ToLower();
            var sort = Filter.SortType == IssueSort.None
                ? null : Filter.SortType.ToString().ToLower();
            var labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;
            return _applicationService.Client.AuthenticatedUser.Issues.GetAll(sort: sort, labels: labels,
                state: state, direction: direction, filter: filter);
        }
    }
}

