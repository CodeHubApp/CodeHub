using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeFramework.Core.Utils;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class IssuesViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly FilterableCollectionViewModel<IssueModel, IssuesFilterModel> _issues;
        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            protected set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }

        public FilterableCollectionViewModel<IssueModel, IssuesFilterModel> Issues
        {
            get { return _issues; }
        }

        public string User { get; private set; }
        public string Slug { get; private set; }

        public IssuesViewModel(string user, string slug)
        {
            User = user;
            Slug = slug;

            _issues = new FilterableCollectionViewModel<IssueModel, IssuesFilterModel>("IssuesViewModel:" + user + "/" + slug);
            _issues.GroupingFunction = GroupModel;
            _issues.Bind(x => x.Filter, async () =>
            {
                IsLoading = true;
                try
                {
                    await Load(false);
                }
                catch (Exception e)
                {
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        public Task Load(bool forceDataRefresh)
        {
            string direction = _issues.Filter.Ascending ? "asc" : "desc";
            string state = _issues.Filter.Open ? "open" : "closed";
            string sort = _issues.Filter.SortType == IssuesFilterModel.Sort.None ? null : _issues.Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(_issues.Filter.Labels) ? null : _issues.Filter.Labels;
            string assignee = string.IsNullOrEmpty(_issues.Filter.Assignee) ? null : _issues.Filter.Assignee;
            string creator = string.IsNullOrEmpty(_issues.Filter.Creator) ? null : _issues.Filter.Creator;
            string mentioned = string.IsNullOrEmpty(_issues.Filter.Mentioned) ? null : _issues.Filter.Mentioned;
            string milestone = _issues.Filter.Milestone == null ? null : _issues.Filter.Milestone.Value;

            var request = Application.Client.Users[User].Repositories[Slug].Issues.GetAll(sort: sort, labels: labels, state: state, direction: direction, 
                                                                                          assignee: assignee, creator: creator, mentioned: mentioned, milestone: milestone);
            return Issues.SimpleCollectionLoad(request, forceDataRefresh);
        }

        private IEnumerable<IGrouping<string, IssueModel>> GroupModel(IEnumerable<IssueModel> model)
        {
            var order = _issues.Filter.SortType;
            if (order == IssuesFilterModel.Sort.Comments)
            {
                var a = _issues.Filter.Ascending ? model.OrderBy(x => x.Comments) : model.OrderByDescending(x => x.Comments);
                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Comments)).ToList();
                return FilterGroup.CreateNumberedGroup(g, "Comments");
            }
            else if (order == IssuesFilterModel.Sort.Updated)
            {
                var a = _issues.Filter.Ascending ? model.OrderBy(x => x.UpdatedAt) : model.OrderByDescending(x => x.UpdatedAt);
                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
                return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Updated");
            }
            else if (order == IssuesFilterModel.Sort.Created)
            {
                var a = _issues.Filter.Ascending ? model.OrderBy(x => x.CreatedAt) : model.OrderByDescending(x => x.CreatedAt);
                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
                return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Created");
            }

            return null;
        }

        public void CreateIssue(IssueModel issue)
        {
            if (!DoesIssueBelong(issue))
                return;
            Issues.Items.Add(issue);
        }

        private bool DoesIssueBelong(IssueModel model)
        {
            if (Issues.Filter == null)
                return true;
            if (Issues.Filter.Open != model.State.Equals("open"))
                return false;
            return true;
        }
    }
}

