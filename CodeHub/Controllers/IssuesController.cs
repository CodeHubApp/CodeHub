using System;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.Dialog;
using System.Linq;
using CodeFramework.Controllers;
using CodeHub.Filters.Models;

namespace CodeHub.Controllers
{
    public class IssuesController : ListController<IssueModel, IssuesFilterModel>
    {
        public string User { get; private set; }
        public string Slug { get; private set; }

        public IssuesController(IListView<IssueModel> view, string user, string slug)
            : base(view)
        {
            User = user;
            Slug = slug;
            Filter = Application.Account.GetFilter<IssuesFilterModel>(this);
        }

        public override void Update(bool force)
        {
            string direction = Filter.Ascending ? "asc" : "desc";
            string state = Filter.Open ? "open" : "closed";
            string sort = Filter.SortType == IssuesFilterModel.Sort.None ? null : Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;
            string assignee = string.IsNullOrEmpty(Filter.Assignee) ? null : Filter.Assignee;
            string creator = string.IsNullOrEmpty(Filter.Creator) ? null : Filter.Creator;
            string mentioned = string.IsNullOrEmpty(Filter.Mentioned) ? null : Filter.Mentioned;

            var response = Application.Client.Users[User].Repositories[Slug].Issues.GetAll(force, sort: sort, labels: labels, state: state, direction: direction, 
                                                                                           assignee: assignee, creator: creator, mentioned: mentioned);
            Model = new ListModel<IssueModel> { Data = response.Data, More = this.CreateMore(response) };
        }

        protected override void SaveFilterAsDefault(IssuesFilterModel filter)
        {
            Application.Account.AddFilter(this, filter);
        }

        public override void ApplyFilter(IssuesFilterModel filter, bool saveAsDefault, bool render)
        {
            //We'll typically just rerender what we have, however, issues require a backend query.
            //So, we'll need to update then render.
            base.ApplyFilter(filter, saveAsDefault, false);

            //This is a hack... Cause I want to display the "loading..." which is on the view
            if (View is BaseControllerDrivenViewController)
                ((BaseControllerDrivenViewController)View).UpdateAndRender();
            else
                UpdateAndRender(false);
        }

        protected override List<IGrouping<string, IssueModel>> GroupModel(List<IssueModel> model, IssuesFilterModel filter)
        {
            var order = filter.SortType;
            if (order == IssuesFilterModel.Sort.Comments)
            {
                var a = filter.Ascending ? model.OrderBy(x => x.Comments) : model.OrderByDescending(x => x.Comments);
                var g = a.GroupBy(x => IntegerCeilings.First(r => r > x.Comments)).ToList();
                return CreateNumberedGroup(g, "Comments");
            }
            else if (order == IssuesFilterModel.Sort.Updated)
            {
                var a = filter.Ascending ? model.OrderBy(x => x.UpdatedAt) : model.OrderByDescending(x => x.UpdatedAt);
                var g = a.GroupBy(x => IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
                return CreateNumberedGroup(g, "Days Ago", "Updated");
            }
            else if (order == IssuesFilterModel.Sort.Created)
            {
                var a = filter.Ascending ? model.OrderBy(x => x.CreatedAt) : model.OrderByDescending(x => x.CreatedAt);
                var g = a.GroupBy(x => IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
                return CreateNumberedGroup(g, "Days Ago", "Created");
            }

            return null;
        }

        public void CreateIssue(IssueModel issue)
        {
            if (!DoesIssueBelong(issue))
                return;
            Model.Data.Add(issue);
            Render();
        }

        public void DeleteIssue(IssueModel issue)
        {
            Model.Data.RemoveAll(a => a.Number == issue.Number);
            Render();
        }

        public void UpdateIssue(IssueModel issue)
        {
            Model.Data.RemoveAll(a => a.Number == issue.Number);
            if (DoesIssueBelong(issue))
                Model.Data.Add(issue);
            Render();
        }

        private bool DoesIssueBelong(IssueModel model)
        {
            if (Filter == null)
                return true;
            if (Filter.Open != model.State.Equals("open"))
                return false;
            return true;
        }
    }
}

