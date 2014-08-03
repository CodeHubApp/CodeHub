using System;
using GitHubSharp.Models;
using CodeHub.Core.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using CodeFramework.Core.Utils;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.ViewModels.PullRequests;

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

        public IReadOnlyReactiveList<IssueModel> Issues { get; private set; }

        public IReactiveCommand GoToIssueCommand { get; private set; }

	    protected BaseIssuesViewModel()
	    {
            Issues = IssuesCollection.CreateDerivedCollection(x => x);

            var gotoIssueCommand = ReactiveCommand.Create();
            gotoIssueCommand.OfType<IssueModel>().Subscribe(x =>
            {
                var isPullRequest = x.PullRequest != null && !(string.IsNullOrEmpty(x.PullRequest.HtmlUrl));
                var s1 = x.Url.Substring(x.Url.IndexOf("/repos/", StringComparison.Ordinal) + 7);
                var repoId = new RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues", StringComparison.Ordinal)));

                if (isPullRequest)
                {
                    var vm = CreateViewModel<PullRequestViewModel>();
                    vm.RepositoryOwner = repoId.Owner;
                    vm.RepositoryName = repoId.Name;
                    vm.PullRequestId = x.Number;
                    ShowViewModel(vm);
                }
                else
                {
                    var vm = CreateViewModel<IssueViewModel>();
                    vm.RepositoryOwner = repoId.Owner;
                    vm.RepositoryName = repoId.Name;
                    vm.IssueId = x.Number;
                    ShowViewModel(vm);
                }
            });
            GoToIssueCommand = gotoIssueCommand;
	    }

        protected virtual List<IGrouping<string, IssueModel>> Group(IEnumerable<IssueModel> model)
		{
			var order = Filter.SortType;
			if (order == BaseIssuesFilterModel.Sort.Comments)
			{
				var a = Filter.Ascending ? model.OrderBy(x => x.Comments) : model.OrderByDescending(x => x.Comments);
				var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Comments)).ToList();
				return FilterGroup.CreateNumberedGroup(g, "Comments");
			}
			if (order == BaseIssuesFilterModel.Sort.Updated)
			{
				var a = Filter.Ascending ? model.OrderBy(x => x.UpdatedAt) : model.OrderByDescending(x => x.UpdatedAt);
				var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
				return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Updated");
			}
			if (order == BaseIssuesFilterModel.Sort.Created)
			{
				var a = Filter.Ascending ? model.OrderBy(x => x.CreatedAt) : model.OrderByDescending(x => x.CreatedAt);
				var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
				return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Created");
			}

            return null;
		}
    }

}

