using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using System;
using ReactiveUI;
using System.Threading.Tasks;
using Octokit;

namespace CodeHub.Core.ViewModels.Issues
{
    public class MyIssuesViewModel : BaseIssuesViewModel
    {
        private readonly ISessionService _sessionService;
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
            private set { this.RaiseAndSetIfChanged(ref _customFilterEnabled, value); }
        }

        public IReactiveCommand<object> GoToFilterCommand { get; private set; }

        public MyIssuesViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            _sessionService = sessionService;

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

            this.WhenAnyValue(x => x.Filter).Skip(1).Subscribe(filter => {
                IssuesBacking.Clear();
                LoadCommand.ExecuteIfCan();
                CustomFilterEnabled = !(filter == _closedFilter || filter == _openFilter);
            });

            GoToFilterCommand = ReactiveCommand.Create();
            GoToFilterCommand.Subscribe(_ => {
                var vm = this.CreateViewModel<MyIssuesFilterViewModel>();
                vm.Init(Filter);
                vm.SaveCommand.Subscribe(filter => Filter = filter);
                NavigateTo(vm);
            });
        }

        protected override bool IssueFilter(Issue issue)
        {
            if (Filter == null)
                return base.IssueFilter(issue);
            if (Filter.Open == IssueState.Open)
                return base.IssueFilter(issue) && issue.State == ItemState.Open;
            if (Filter.Open == IssueState.Closed)
                return base.IssueFilter(issue) && issue.State == ItemState.Closed;
            return base.IssueFilter(issue);
        }

        protected override void AddRequestParameters(System.Collections.Generic.IDictionary<string, string> parameters)
        {
            parameters["filter"] = Filter.FilterType.ToString().ToLower();
            parameters["direction"] = Filter.Ascending ? "asc" : "desc";
            parameters["state"] = Filter.Open.ToString().ToLower();
            parameters["sort"] = Filter.SortType == CodeHub.Core.Filters.IssueSort.None ? null : Filter.SortType.ToString().ToLower();
            parameters["labels"] = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;
        }

        protected override Uri RequestUri
        {
            get { return ApiUrls.Issues(); }
        }
    }
}

