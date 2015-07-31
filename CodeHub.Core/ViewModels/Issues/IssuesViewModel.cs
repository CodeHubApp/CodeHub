using System;
using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using ReactiveUI;
using CodeHub.Core.Factories;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssuesViewModel : BaseIssuesViewModel
    {
        private readonly ISessionService _sessionService;

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public IReactiveCommand<object> GoToNewIssueCommand { get; private set; }

        public IReactiveCommand<object> GoToCustomFilterCommand { get; private set; }

        public RepositoryIssuesFilterViewModel Filter { get; private set; }

        private IssueFilterSelection _filterSelection;
        public IssueFilterSelection FilterSelection
        {
            get { return _filterSelection; }
            set
            {
                if (value == IssueFilterSelection.Open)
                    Filter.SetDefault(true).ToBackground();
                else if (value == IssueFilterSelection.Closed)
                    Filter.SetDefault(false).ToBackground();
                else if (value == IssueFilterSelection.Mine)
                    Filter.SetDefault(true, _sessionService.Account.Username).ToBackground();
                this.RaiseAndSetIfChanged(ref _filterSelection, value);
            }
        }

        protected override bool IssueFilter(GitHubSharp.Models.IssueModel issue)
        {
            IssueState issueState;
            if (!Enum.TryParse(issue.State, true, out issueState))
                return base.IssueFilter(issue);
            
            return base.IssueFilter(issue) && (Filter.State == issueState);
        }

        public IssuesViewModel(ISessionService sessionService, IActionMenuFactory actionMenuFactory)
            : base(sessionService)
	    {
            _sessionService = sessionService;
            Filter = new RepositoryIssuesFilterViewModel(sessionService, actionMenuFactory);

            Title = "Issues";

            GoToNewIssueCommand = ReactiveCommand.Create();
	        GoToNewIssueCommand.Subscribe(_ => {
	            var vm = this.CreateViewModel<IssueAddViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
                vm.SaveCommand.Subscribe(x => LoadCommand.ExecuteIfCan());
                NavigateTo(vm);
	        });

            Filter.SaveCommand.Subscribe(_ => {
                IssuesBacking.Clear();
                LoadCommand.ExecuteIfCan();
            });

            GoToCustomFilterCommand = ReactiveCommand.Create();
            GoToCustomFilterCommand.Subscribe(_ => FilterSelection = IssueFilterSelection.Custom);
	    }

        public IssuesViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = Filter.RepositoryOwner = repositoryOwner;
            RepositoryName = Filter.RepositoryName = repositoryName;
            return this;
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.IssueModel>> CreateRequest()
        {
            var direction = Filter.Ascending ? "asc" : "desc";
            var state = Filter.State.ToString().ToLower();
            var sort = Filter.SortType == IssueSort.None ? null : Filter.SortType.ToString().ToLower();
            var creator = string.IsNullOrEmpty(Filter.Creator) ? null : Filter.Creator;
            var mentioned = string.IsNullOrEmpty(Filter.Mentioned) ? null : Filter.Mentioned;
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

