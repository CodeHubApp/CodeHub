using System;
using GitHubSharp.Models;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.Utilities;
using Xamarin.Utilities.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class BaseIssuesViewModel : BaseViewModel, IBaseIssuesViewModel
    {
        protected readonly ReactiveList<IssueModel> IssuesCollection = new ReactiveList<IssueModel>();

        public IReadOnlyReactiveList<IssueItemViewModel> Issues { get; private set; }

        public IReactiveCommand GoToIssueCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected BaseIssuesViewModel()
	    {
            Issues = IssuesCollection.CreateDerivedCollection(x => CreateItemViewModel(x), 
                x => x.Title.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            var gotoIssueCommand = ReactiveCommand.Create();
            gotoIssueCommand.OfType<IssueItemViewModel>().Where(x => x.IsPullRequest).Subscribe(x =>
            {
                var vm = this.CreateViewModel<PullRequestViewModel>();
                vm.RepositoryOwner = x.RepositoryOwner;
                vm.RepositoryName = x.RepositoryName;
                vm.Id = (int)x.Issue.Number;
                NavigateTo(vm);

            });
            gotoIssueCommand.OfType<IssueItemViewModel>().Where(x => !x.IsPullRequest).Subscribe(x =>
            {
                var vm = this.CreateViewModel<IssueViewModel>();
                vm.RepositoryOwner = x.RepositoryOwner;
                vm.RepositoryName = x.RepositoryName;
                vm.Id = (int)x.Issue.Number;
                NavigateTo(vm);
            });
            GoToIssueCommand = gotoIssueCommand;
	    }

        private static IssueItemViewModel CreateItemViewModel(IssueModel x)
        {
            var isPullRequest = x.PullRequest != null && !(string.IsNullOrEmpty(x.PullRequest.HtmlUrl));
            var s1 = x.Url.Substring(x.Url.IndexOf("/repos/", StringComparison.Ordinal) + 7);
            var repoId = new RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues", StringComparison.Ordinal)));

            return new IssueItemViewModel
            {
                Issue = x,
                RepositoryFullName = repoId.Owner + "/" + repoId.Name,
                RepositoryName = repoId.Name,
                RepositoryOwner = repoId.Owner,
                IsPullRequest = isPullRequest
            };
        }
    }
}

