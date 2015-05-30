using System;
using GitHubSharp.Models;
using System.Reactive.Linq;
using System.Linq;
using ReactiveUI;
using CodeHub.Core.ViewModels.PullRequests;
using System.Collections.Generic;
using System.Reactive;
using GitHubSharp;
using System.Threading.Tasks;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class BaseIssuesViewModel : BaseViewModel, IBaseIssuesViewModel, IPaginatableViewModel
    {
        private readonly ISessionService _sessionService;
        protected readonly IReactiveList<IssueModel> IssuesBacking = new ReactiveList<IssueModel>();

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

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                return IssuesBacking.SimpleCollectionLoad(CreateRequest(), 
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x()));
            });
	    }

        protected virtual bool IssueFilter(IssueModel issue)
        {
            return issue.Title.ContainsKeyword(SearchKeyword);
        }

        private IssueItemViewModel CreateItemViewModel(IssueModel issue)
        {
            var item = new IssueItemViewModel(issue);
            if (item.IsPullRequest)
            {
                item.GoToCommand.Subscribe(_ => {
                    var vm = this.CreateViewModel<PullRequestViewModel>();
                    vm.Init(item.RepositoryOwner, item.RepositoryName, item.Number);
                    vm.IssueUpdated.Subscribe(x => UpdateIssue(x));
                    NavigateTo(vm);
                });
            }
            else
            {
                item.GoToCommand.Subscribe(_ => {
                    var vm = this.CreateViewModel<IssueViewModel>();
                    vm.Init(item.RepositoryOwner, item.RepositoryName, item.Number);
                    vm.IssueUpdated.Subscribe(x => UpdateIssue(x));
                    NavigateTo(vm);
                });
            }

            return item;
        }

        private async Task UpdateIssue(Octokit.Issue issue)
        {
            var localIssue = IssuesBacking.FirstOrDefault(x => string.Equals(x.Url, issue.Url.AbsoluteUri, StringComparison.OrdinalIgnoreCase));
            if (localIssue == null)
                return;

            var index = IssuesBacking.IndexOf(localIssue);
            if (index < 0)
                return;

            var matches = System.Text.RegularExpressions.Regex.Matches(issue.Url.AbsolutePath, "/repos/([^/]+)/([^/]+)/.+");
            if (matches.Count != 1 || matches[0].Groups.Count != 3)
                return;

            var req = _sessionService.Client.Users[matches[0].Groups[1].Value].Repositories[matches[0].Groups[2].Value].Issues[issue.Number].Get();
            IssuesBacking[index] = (await _sessionService.Client.ExecuteAsync(req)).Data;
            IssuesBacking.Reset();
        }

        protected abstract GitHubRequest<List<IssueModel>> CreateRequest();
    }
}

