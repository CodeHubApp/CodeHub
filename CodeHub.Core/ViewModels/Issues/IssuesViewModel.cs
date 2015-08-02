using System;
using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Linq;
using System.Reactive.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssuesViewModel : BaseIssuesViewModel
    {
        private readonly ISessionService _sessionService;

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public IReactiveCommand<object> GoToNewIssueCommand { get; }

        public IReactiveCommand<object> GoToFilterCommand { get; }

        private IssuesFilterModel _filter;
        public IssuesFilterModel Filter
        {
            get { return _filter; }
            private set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        private IssueFilterSelection _filterSelection;
        public IssueFilterSelection FilterSelection
        {
            get { return _filterSelection; }
            set
            {
                if (value == _filterSelection)
                    return;
                
                if (value == IssueFilterSelection.Open)
                {
                    Filter = IssuesFilterModel.CreateOpenFilter();
                }
                else if (value == IssueFilterSelection.Closed)
                {
                    Filter = IssuesFilterModel.CreateClosedFilter();
                }
                else if (value == IssueFilterSelection.Mine)
                {
                    _sessionService.GitHubClient.User.Current()
                        .ToObservable()
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => Filter = IssuesFilterModel.CreateMineFilter(x));
                }

                this.RaiseAndSetIfChanged(ref _filterSelection, value);
            }
        }

        protected override bool IssueFilter(GitHubSharp.Models.IssueModel issue)
        {
            IssueState issueState;
            if (!Enum.TryParse(issue.State, true, out issueState))
                return base.IssueFilter(issue);
            
            return base.IssueFilter(issue) && (Filter.IssueState == issueState);
        }

        public IssuesViewModel(ISessionService sessionService)
            : base(sessionService)
	    {
            _sessionService = sessionService;
            Filter = new IssuesFilterModel();

            Title = "Issues";

            GoToNewIssueCommand = ReactiveCommand.Create();
	        GoToNewIssueCommand.Subscribe(_ => {
	            var vm = this.CreateViewModel<IssueAddViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
                vm.SaveCommand.Subscribe(x => LoadCommand.ExecuteIfCan());
                NavigateTo(vm);
	        });

            this.WhenAnyValue(x => x.Filter).Skip(1).Subscribe(filter => {
                IssuesBacking.Clear();
                LoadCommand.ExecuteIfCan();
                //CustomFilterEnabled = !(filter == _closedFilter || filter == _openFilter);
            });

            GoToFilterCommand = ReactiveCommand.Create();
            GoToFilterCommand.Subscribe(_ => {
                var vm = this.CreateViewModel<RepositoryIssuesFilterViewModel>();
                vm.Init(RepositoryOwner, RepositoryName, Filter);
                vm.SaveCommand.Subscribe(filter => {
                    Filter = filter;
                    FilterSelection = IssueFilterSelection.Custom;
                });
                NavigateTo(vm);
            });
	    }

        public IssuesViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.IssueModel>> CreateRequest()
        {
            var direction = Filter.Ascending ? "asc" : "desc";
            var state = Filter.IssueState.ToString().ToLower();
            var sort = Filter.SortType == IssueSort.None ? null : Filter.SortType.ToString().ToLower();
            var creator = Filter.Creator;
            var mentioned = Filter.Mentioned;
            var labels = Filter.Labels?.Count > 0 ? string.Join(",", Filter.Labels.Select(x => x.Name)) : null;
            var milestone = Filter.Milestone?.Number.ToString();
            var assignee = Filter.Assignee?.Login;

            return _sessionService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues.GetAll(
                sort: sort, labels: labels, state: state, direction: direction,
                assignee: assignee, creator: creator, mentioned: mentioned, milestone: milestone);
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

