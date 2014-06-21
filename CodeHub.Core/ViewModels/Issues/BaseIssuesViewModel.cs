using System;
using GitHubSharp.Models;
using CodeHub.Core.Filters;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.Core.Utils;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class BaseIssuesViewModel<TFilterModel> : LoadableViewModel, IBaseIssuesViewModel where TFilterModel : BaseIssuesFilterModel
    {
        private TFilterModel _filter;

		public ReactiveCollection<IssueModel> Issues { get; private set; }

        public IReactiveCommand GoToIssueCommand { get; private set; }

        public TFilterModel Filter
	    {
	        get { return _filter; }
	        set { this.RaiseAndSetIfChanged(ref _filter, value); }
	    }

	    protected BaseIssuesViewModel()
	    {
	        Issues = new ReactiveCollection<IssueModel>();
	    }

//		{
//			get 
//			{ 
//				return new MvxCommand<IssueModel>(x =>
//				{
//					var isPullRequest = x.PullRequest != null && !(string.IsNullOrEmpty(x.PullRequest.HtmlUrl));
//					var s1 = x.Url.Substring(x.Url.IndexOf("/repos/") + 7);
//					var repoId = new RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues")));
//
//					if (isPullRequest)
//						ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = repoId.Owner, Repository = repoId.Name, Id = x.Number });
//					else
//						ShowViewModel<IssueViewModel>(new IssueViewModel.NavObject { Username = repoId.Owner, Repository = repoId.Name, Id = x.Number });
//				});
//			}
//		}

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

    public interface IBaseIssuesViewModel
    {
        ReactiveCollection<IssueModel> Issues { get; }

        IReactiveCommand GoToIssueCommand { get; }
    }
}

