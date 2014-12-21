using System;
using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssuesViewModel : BaseIssuesViewModel, ILoadableViewModel
    {
        private readonly IssuesFilterModel _openFilter = IssuesFilterModel.CreateOpenFilter();
        private readonly IssuesFilterModel _closedFilter = IssuesFilterModel.CreateClosedFilter();
        private readonly IssuesFilterModel _mineFilter;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IReactiveCommand<object> GoToNewIssueCommand { get; private set; }

        public IReactiveCommand<object> GoToCustomFilterCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private IssuesFilterModel _filter;
        private IssuesFilterModel Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        private readonly ObservableAsPropertyHelper<IssueFilterSelection> _filterSelection;
        public IssueFilterSelection FilterSelection
        {
            get { return _filterSelection.Value; }
            set
            {
                if (value == IssueFilterSelection.Open)
                    Filter = _openFilter;
                else if (value == IssueFilterSelection.Closed)
                    Filter = _closedFilter;
                else if (value == IssueFilterSelection.Mine)
                    Filter = _mineFilter;
            }
        }

	    public IssuesViewModel(IApplicationService applicationService)
	    {
            _mineFilter = IssuesFilterModel.CreateMineFilter(applicationService.Account.Username);

            Title = "Issues";

            _filterSelection = this.WhenAnyValue(x => x.Filter)
                .Select(x =>
                {
                    if (x == null || _openFilter.Equals(x))
                        return IssueFilterSelection.Open;
                    if (_closedFilter.Equals(x))
                        return IssueFilterSelection.Closed;
                    if (_mineFilter.Equals(x))
                        return IssueFilterSelection.Mine;
                    return IssueFilterSelection.Custom;
                })
                .ToProperty(this, Xamarin => Xamarin.FilterSelection);

            GoToNewIssueCommand = ReactiveCommand.Create();
	        GoToNewIssueCommand.Subscribe(_ =>
	        {
	            var vm = this.CreateViewModel<IssueAddViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
                vm.CreatedIssue.IsNotNull().Subscribe(IssuesCollection.Add);
                NavigateTo(vm);
	        });

            GoToCustomFilterCommand = ReactiveCommand.Create();

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
	        {
	            var direction = Filter.Ascending ? "asc" : "desc";
	            var state = Filter.Open ? "open" : "closed";
	            var sort = Filter.SortType == BaseIssuesFilterModel.Sort.None ? null : Filter.SortType.ToString().ToLower();
	            var labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;
	            var assignee = string.IsNullOrEmpty(Filter.Assignee) ? null : Filter.Assignee;
	            var creator = string.IsNullOrEmpty(Filter.Creator) ? null : Filter.Creator;
	            var mentioned = string.IsNullOrEmpty(Filter.Mentioned) ? null : Filter.Mentioned;
	            var milestone = Filter.Milestone == null ? null : Filter.Milestone.Value;

	            var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues.GetAll(
	                sort: sort, labels: labels, state: state, direction: direction,
	                assignee: assignee, creator: creator, mentioned: mentioned, milestone: milestone);
                return IssuesCollection.SimpleCollectionLoad(request, t as bool?);
	        });
	    }

        public enum IssueFilterSelection
        {
            Open = 0,
            Closed,
            Mine,
            Custom
        }
    }
}

