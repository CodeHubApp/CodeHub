using System;
using System.Reactive.Linq;
using System.Linq;
using ReactiveUI;
using CodeHub.Core.ViewModels.PullRequests;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using Octokit;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class BaseIssuesViewModel : BaseViewModel, IProvidesSearchKeyword, IPaginatableViewModel
    {
        private readonly ISessionService _sessionService;
        protected readonly IReactiveList<Issue> IssuesBacking = new ReactiveList<Issue>(resetChangeThreshold: 1.0);

        public IReadOnlyReactiveList<IssueItemViewModel> Issues { get; private set; }

        private IList<IssueGroupViewModel> _groupedIssues;
        public IList<IssueGroupViewModel> GroupedIssues
        {
            get { return _groupedIssues; }
            private set { this.RaiseAndSetIfChanged(ref _groupedIssues, value); }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<Unit> LoadMoreCommand { get; private set; }

        protected BaseIssuesViewModel(ISessionService sessionService)
	    {
            _sessionService = sessionService;

            Issues = IssuesBacking.CreateDerivedCollection(
                x => CreateItemViewModel(x),
                filter: IssueFilter, 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            Issues.Changed.Subscribe(_ =>
            {
                GroupedIssues = Issues.GroupBy(x => x.RepositoryFullName)
                    .Select(x => new IssueGroupViewModel(x.Key, x)).ToList();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                IssuesBacking.Reset(await RetrieveIssues());
            });
	    }

        protected virtual bool IssueFilter(Issue issue)
        {
            return issue.Title.ContainsKeyword(SearchKeyword);
        }

        private IssueItemViewModel CreateItemViewModel(Issue issue)
        {
            var item = new IssueItemViewModel(issue);
            if (item.IsPullRequest)
            {
                item.GoToCommand.Subscribe(_ => {
                    var vm = this.CreateViewModel<PullRequestViewModel>();
                    vm.Init(item.RepositoryOwner, item.RepositoryName, item.Number, issue: issue);
                    vm.IssueUpdated.Subscribe(x => UpdateIssue(x));
                    NavigateTo(vm);
                });
            }
            else
            {
                item.GoToCommand.Subscribe(_ => {
                    var vm = this.CreateViewModel<IssueViewModel>();
                    vm.Init(item.RepositoryOwner, item.RepositoryName, item.Number, issue);
                    vm.IssueUpdated.Subscribe(x => UpdateIssue(x));
                    NavigateTo(vm);
                });
            }

            return item;
        }

        private async Task UpdateIssue(Issue issue)
        {
            var localIssue = IssuesBacking.FirstOrDefault(x => x.Url == issue.Url);
            if (localIssue == null)
                return;

            var index = IssuesBacking.IndexOf(localIssue);
            if (index < 0)
                return;

            var matches = System.Text.RegularExpressions.Regex.Matches(issue.Url.AbsolutePath, "/repos/([^/]+)/([^/]+)/.+");
            if (matches.Count != 1 || matches[0].Groups.Count != 3)
                return;

            IssuesBacking[index] = await _sessionService.GitHubClient.Issue.Get(matches[0].Groups[1].Value, matches[0].Groups[2].Value, issue.Number);
            IssuesBacking.Reset();
        }

        private async Task<IReadOnlyList<Issue>> RetrieveIssues(int page = 1)
        {
            var connection = _sessionService.GitHubClient.Connection;
            var parameters = new Dictionary<string, string>();
            parameters["page"] = page.ToString();
            parameters["per_page"] = 50.ToString();
            AddRequestParameters(parameters);

            parameters = parameters.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
            var ret = await connection.Get<IReadOnlyList<Issue>>(RequestUri, parameters, "application/json");

            if (ret.HttpResponse.ApiInfo.Links.ContainsKey("next"))
            {
                LoadMoreCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                    IssuesBacking.AddRange(await RetrieveIssues(page + 1));
                });
            }
            else
            {
                LoadMoreCommand = null;
            }

            return ret.Body;
        }

        protected virtual void AddRequestParameters(IDictionary<string, string> parameters)
        {
        }

        protected abstract Uri RequestUri { get; }
    }
}

