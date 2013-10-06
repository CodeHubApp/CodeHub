using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Filters.Models;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.Controllers
{
    public class MyIssuesController : ListController<IssueModel, MyIssuesFilterModel>
    {
        public MyIssuesController(IListView<IssueModel> view)
            : base(view)
        {
            Filter = Application.Account.GetFilter<MyIssuesFilterModel>(this);
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            string filter = Filter.FilterType.ToString().ToLower();
            string direction = Filter.Ascending ? "asc" : "desc";
            string state = Filter.Open ? "open" : "closed";
            string sort = Filter.SortType == MyIssuesFilterModel.Sort.None ? null : Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;

            var request = Application.Client.AuthenticatedUser.Issues.GetAll(sort: sort, labels: labels, state: state, direction: direction, filter: filter);
            this.RequestModel(request, forceDataRefresh, response => {
                RenderView(new ListModel<IssueModel>(response.Data, this.CreateMore(response)));
            });
        }
        
        protected override List<IGrouping<string, IssueModel>> GroupModel(List<IssueModel> model, MyIssuesFilterModel filter)
        {
            var order = filter.SortType;
            if (order == MyIssuesFilterModel.Sort.Comments)
            {
                var a = filter.Ascending ? model.OrderBy(x => x.Comments) : model.OrderByDescending(x => x.Comments);
                var g = a.GroupBy(x => IntegerCeilings.First(r => r > x.Comments)).ToList();
                return CreateNumberedGroup(g, "Comments");
            }
            else if (order == MyIssuesFilterModel.Sort.Updated)
            {
                var a = filter.Ascending ? model.OrderBy(x => x.UpdatedAt) : model.OrderByDescending(x => x.UpdatedAt);
                var g = a.GroupBy(x => IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
                return CreateNumberedGroup(g, "Days Ago", "Updated");
            }
            else if (order == MyIssuesFilterModel.Sort.Created)
            {
                var a = filter.Ascending ? model.OrderBy(x => x.CreatedAt) : model.OrderByDescending(x => x.CreatedAt);
                var g = a.GroupBy(x => IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
                return CreateNumberedGroup(g, "Days Ago", "Created");
            }

            return null;
        }

        protected override void SaveFilterAsDefault(MyIssuesFilterModel filter)
        {
            Application.Account.AddFilter(this, filter);
        }

        public override void ApplyFilter(MyIssuesFilterModel filter, bool saveAsDefault, bool render)
        {
            //We'll typically just rerender what we have, however, issues require a backend query.
            //So, we'll need to update then render.
            base.ApplyFilter(filter, saveAsDefault, false);
            Update(false);
        }

        public void DeleteIssue(IssueModel issue)
        {
            Model.Data.RemoveAll(a => a.Number == issue.Number);
            RenderView();
        }

        public void UpdateIssue(IssueModel issue)
        {
            Model.Data.RemoveAll(a => a.Number == issue.Number);
            Model.Data.Add(issue);
            RenderView();
        }
    }
}

