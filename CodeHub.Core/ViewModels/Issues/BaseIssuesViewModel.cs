using System;
using GitHubSharp.Models;
using System.Reactive.Linq;
using System.Linq;
using ReactiveUI;
using CodeHub.Core.ViewModels.PullRequests;
using System.Collections.Generic;
using System.Reactive;
using GitHubSharp;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class BaseIssuesViewModel : BaseViewModel, IBaseIssuesViewModel, IPaginatableViewModel
    {
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

        protected BaseIssuesViewModel()
	    {
            Issues = IssuesBacking.CreateDerivedCollection(
                x => CreateItemViewModel(x),
                filter: x => x.Title.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            Issues.Changed.Subscribe(_ =>
            {
                GroupedIssues = Issues.GroupBy(x => x.RepositoryFullName)
                    .Select(x => new IssueGroupViewModel(x.Key, x)).ToList();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                return IssuesBacking.SimpleCollectionLoad(CreateRequest(), t as bool?, 
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x()));
            });
	    }

        private IssueItemViewModel CreateItemViewModel(IssueModel issue)
        {
            var item = new IssueItemViewModel(issue);
            if (item.IsPullRequest)
            {
                item.GoToCommand.Subscribe(_ =>
                {
                    var vm = this.CreateViewModel<PullRequestViewModel>();
                    vm.RepositoryOwner = item.RepositoryOwner;
                    vm.RepositoryName = item.RepositoryName;
                    vm.Id = item.Number;
                    NavigateTo(vm);
                });
            }
            else
            {
                item.GoToCommand.Subscribe(_ =>
                {
                    var vm = this.CreateViewModel<IssueViewModel>();
                    vm.RepositoryOwner = item.RepositoryOwner;
                    vm.RepositoryName = item.RepositoryName;
                    vm.Id = item.Number;
                    NavigateTo(vm);
                });
            }

            return item;
        }

        protected abstract GitHubRequest<List<IssueModel>> CreateRequest();
    }
}

