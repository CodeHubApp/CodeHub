using System;
using GitHubSharp.Models;
using CodeHub.Core.Filters;
using System.Reactive.Linq;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class BaseIssuesViewModel<TFilterModel> : BaseViewModel, IBaseIssuesViewModel where TFilterModel : BaseIssuesFilterModel
    {
        protected readonly ReactiveList<IssueModel> IssuesCollection = new ReactiveList<IssueModel>();
        private TFilterModel _filter;

        public TFilterModel Filter
	    {
	        get { return _filter; }
	        set { this.RaiseAndSetIfChanged(ref _filter, value); }
	    }

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
            Issues = IssuesCollection.CreateDerivedCollection(x => 
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
                }, 
                x => x.Title.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            var gotoIssueCommand = ReactiveCommand.Create();
            gotoIssueCommand.OfType<IssueItemViewModel>().Where(x => x.IsPullRequest).Subscribe(x =>
            {
                var vm = CreateViewModel<PullRequestViewModel>();
                vm.RepositoryOwner = x.RepositoryOwner;
                vm.RepositoryName = x.RepositoryName;
                vm.PullRequestId = x.Issue.Number;
                ShowViewModel(vm);

            });
            gotoIssueCommand.OfType<IssueItemViewModel>().Where(x => !x.IsPullRequest).Subscribe(x =>
            {
                var vm = CreateViewModel<IssueViewModel>();
                vm.RepositoryOwner = x.RepositoryOwner;
                vm.RepositoryName = x.RepositoryName;
                vm.IssueId = x.Issue.Number;
                ShowViewModel(vm);
            });
            GoToIssueCommand = gotoIssueCommand;
	    }
    }
}

