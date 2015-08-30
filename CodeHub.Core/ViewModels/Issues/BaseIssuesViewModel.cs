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
    public abstract class BaseIssuesViewModel : BaseSearchableListViewModel<Issue, IssueItemViewModel>
    {
        private readonly ISessionService _sessionService;

        private IList<IssueGroupViewModel> _groupedIssues;
        public IList<IssueGroupViewModel> GroupedIssues
        {
            get { return _groupedIssues; }
            private set { this.RaiseAndSetIfChanged(ref _groupedIssues, value); }
        }

        protected BaseIssuesViewModel(ISessionService sessionService)
	    {
            _sessionService = sessionService;

            Items = InternalItems.CreateDerivedCollection(
                x => CreateItemViewModel(x),
                filter: IssueFilter, 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            Items.Changed.Subscribe(_ => {
                GroupedIssues = Items.GroupBy(x => x.RepositoryFullName)
                    .Select(x => new IssueGroupViewModel(x.Key, x)).ToList();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                InternalItems.Reset(await RetrieveIssues());
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
            var localIssue = InternalItems.FirstOrDefault(x => x.Url == issue.Url);
            if (localIssue == null)
                return;

            var index = InternalItems.IndexOf(localIssue);
            if (index < 0)
                return;

            var matches = System.Text.RegularExpressions.Regex.Matches(issue.Url.AbsolutePath, "/repos/([^/]+)/([^/]+)/.+");
            if (matches.Count != 1 || matches[0].Groups.Count != 3)
                return;

            InternalItems[index] = await _sessionService.GitHubClient.Issue.Get(matches[0].Groups[1].Value, matches[0].Groups[2].Value, issue.Number);
            InternalItems.Reset();
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
                    InternalItems.AddRange(await RetrieveIssues(page + 1));
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

