using System;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.Filters;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels.PullRequests;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Core.Utils;

namespace CodeHub.Core.ViewModels.Issues
{
	public abstract class BaseIssuesViewModel<TFilterModel> : LoadableViewModel, IBaseIssuesViewModel where TFilterModel : BaseIssuesFilterModel<TFilterModel>, new()
    {
		protected FilterableCollectionViewModel<IssueModel, TFilterModel> _issues;

		public FilterableCollectionViewModel<IssueModel, TFilterModel> Issues
		{
			get { return _issues; }
		}

		public ICommand GoToIssueCommand
		{
			get 
			{ 
				return new MvxCommand<IssueModel>(x =>
				{
					var isPullRequest = x.PullRequest != null && !(string.IsNullOrEmpty(x.PullRequest.HtmlUrl));
					var s1 = x.Url.Substring(x.Url.IndexOf("/repos/") + 7);
					var repoId = new RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues")));

					if (isPullRequest)
						ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = repoId.Owner, Repository = repoId.Name, Id = x.Number });
					else
						ShowViewModel<IssueViewModel>(new IssueViewModel.NavObject { Username = repoId.Owner, Repository = repoId.Name, Id = x.Number });
				});
			}
		}

        protected virtual List<IGrouping<string, IssueModel>> Group(IEnumerable<IssueModel> model)
		{
			var order = Issues.Filter.SortType;
			if (order == BaseIssuesFilterModel<TFilterModel>.Sort.Comments)
			{
				var a = Issues.Filter.Ascending ? model.OrderBy(x => x.Comments) : model.OrderByDescending(x => x.Comments);
				var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Comments)).ToList();
				return FilterGroup.CreateNumberedGroup(g, "Comments");
			}
			if (order == BaseIssuesFilterModel<TFilterModel>.Sort.Updated)
			{
				var a = Issues.Filter.Ascending ? model.OrderBy(x => x.UpdatedAt) : model.OrderByDescending(x => x.UpdatedAt);
				var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
				return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Updated");
			}
			if (order == BaseIssuesFilterModel<TFilterModel>.Sort.Created)
			{
				var a = Issues.Filter.Ascending ? model.OrderBy(x => x.CreatedAt) : model.OrderByDescending(x => x.CreatedAt);
				var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
				return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Created");
			}

            return null;
		}
    }

	public interface IBaseIssuesViewModel : IMvxViewModel
	{
		ICommand GoToIssueCommand { get; }
	}
}

