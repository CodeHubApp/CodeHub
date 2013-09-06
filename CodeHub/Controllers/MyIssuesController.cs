using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Filters.Models;

namespace CodeHub.Controllers
{
    public class MyIssuesController : ListController<IssueModel, MyIssuesFilterModel>
    {
        public MyIssuesController(IListView<IssueModel> view)
            : base(view)
        {
            Filter = Application.Account.GetFilter<MyIssuesFilterModel>(this);
        }

        public override void Update(bool force)
        {
            string filter = Filter.FilterType.ToString().ToLower();
            string direction = Filter.Ascending ? "asc" : "desc";
            string state = Filter.Open ? "open" : "closed";
            string sort = Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;

            var response = Application.Client.AuthenticatedUser.Issues.GetAll(force, sort: sort, labels: labels, state: state, direction: direction, filter: filter);
            Model = new ListModel<IssueModel> { Data = response.Data, More = this.CreateMore(response) };
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
        }

        public void DeleteIssue(IssueModel issue)
        {
            Model.Data.RemoveAll(a => a.Number == issue.Number);
            Render();
        }

        public void UpdateIssue(IssueModel issue)
        {
            Model.Data.RemoveAll(a => a.Number == issue.Number);
            Model.Data.Add(issue);
            Render();
        }
    }
}

