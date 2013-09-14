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
            string sort = Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;

            var response = Application.Client.Users[User].Repositories[Slug].Issues.GetAll(force, sort: sort, labels: labels, state: state, direction: direction);
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
            //            var order = (IssuesFilterModel.Order)Filter.OrderBy;
            //            if (order == IssuesFilterModel.Order.Status)
            //            {
            //                return model.GroupBy(x => x.Status).ToList();
            //            }
            //            else if (order == IssuesFilterModel.Order.Priority)
            //            {
            //                return model.GroupBy(x => x.Priority).ToList();
            //            }
            //            else if (order == IssuesFilterModel.Order.Utc_Last_Updated)
            //            {
            //                var a = model.OrderByDescending(x => x.UtcLastUpdated).GroupBy(x => IntegerCeilings.First(r => r > x.UtcLastUpdated.TotalDaysAgo()));
            //                return CreateNumberedGroup(a, "Days Ago", "Updated");
            //            }
            //            else if (order == IssuesFilterModel.Order.Created_On)
            //            {
            //                var a = model.OrderByDescending(x => x.UtcCreatedOn).GroupBy(x => IntegerCeilings.First(r => r > x.UtcCreatedOn.TotalDaysAgo()));
            //                return CreateNumberedGroup(a, "Days Ago", "Created");
            //            }
            //            else if (order == IssuesFilterModel.Order.Version)
            //            {
            //                return model.GroupBy(x => (x.Metadata != null && !string.IsNullOrEmpty(x.Metadata.Version)) ? x.Metadata.Version : "No Version").ToList();
            //            }
            //            else if (order == IssuesFilterModel.Order.Component)
            //            {
            //                return model.GroupBy(x => (x.Metadata != null && !string.IsNullOrEmpty(x.Metadata.Component)) ? x.Metadata.Component : "No Component").ToList();
            //            }
            //            else if (order == IssuesFilterModel.Order.Milestone)
            //            {
            //                return model.GroupBy(x => (x.Metadata != null && !string.IsNullOrEmpty(x.Metadata.Milestone)) ? x.Metadata.Milestone : "No Milestone").ToList();
            //            }
            //
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
            Model.Data.Add(issue);
            Render();
        }


        private bool DoesIssueBelong(IssueModel model)
        {
            if (Filter == null)
                return true;

            //            if (Filter.Status != null && !Filter.Status.IsDefault())
            //                if (!FieldToUrl(null, Filter.Status).Any(x => x.Item2.Equals(model.Status)))
            //                    return false;
            //            if (Filter.Kind != null && !Filter.Kind.IsDefault())
            //                if (!FieldToUrl(null, Filter.Kind).Any(x => x.Item2.Equals(model.Metadata.Kind)))
            //                    return false;
            //            if (Filter.Priority != null && !Filter.Priority.IsDefault())
            //                if (!FieldToUrl(null, Filter.Priority).Any(x => x.Item2.Equals(model.Priority)))
            //                    return false;
            //            if (!string.IsNullOrEmpty(Filter.AssignedTo))
            //                if (!object.Equals(Filter.AssignedTo, (model.Responsible == null ? "unassigned" : model.Responsible.Username)))
            //                    return false;
            //            if (!string.IsNullOrEmpty(Filter.ReportedBy))
            //                if (model.ReportedBy == null || !object.Equals(Filter.ReportedBy, model.ReportedBy.Username))
            //                    return false;
            //
            return true;
        }

        private void ChildChangedModel(IssueModel changedModel, IssueModel oldModel)
        {
            //            //If null then it's been deleted!
            //            if (changedModel == null)
            //            {
            //                var c = TableView.ContentOffset;
            //                var m = Model as List<IssueModel>;
            //                m.RemoveAll(a => a.LocalId == oldModel.LocalId);
            //
            //                Render();
            //                TableView.ContentOffset = c;
            //            }
            //            else
            //            {
            //                if (DoesIssueBelong(changedModel))
            //                {
            //                    AddItems(new List<IssueModel>(1) { changedModel });
            //                    ScrollToModel(oldModel);
            //                }
            //                else
            //                {
            //                    var c = TableView.ContentOffset;
            //                    var m = Model as List<IssueModel>;
            //                    m.RemoveAll(a => a.LocalId == changedModel.LocalId);
            //                    Render();
            //                    TableView.ContentOffset = c;
            //                }
            //            }
        }

        private List<Section> CreateSection(IEnumerable<IGrouping<string, IssueModel>> results)
        {
            var sections = new List<Section>();
            //            InvokeOnMainThread(() => {
            //                foreach (var groups in results)
            //                {
            //                    var sec = new Section(new TableViewSectionView(groups.Key));
            //                    sections.Add(sec);
            //                    foreach (var y in groups)
            //                        sec.Add(CreateElement(y));
            //                }
            //            });
            return sections;
        }

        private void AddItems(List<IssueModel> issues)
        {
            //            if (Model == null)
            //                Model = issues;
            //            else
            //            {
            //                //Remove any duplicates
            //                var model = Model as List<IssueModel>;
            //                model.RemoveAll(x => issues.Any(y => y.LocalId == x.LocalId));
            //                model.AddRange(issues);
            //            }
            //
            //            //Refresh this 
            //            Render();
        }

    }
}

