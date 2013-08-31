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
            var data = Application.Client.Users[User].Repositories[Slug].Issues.GetAll(force);
            Model = new ListModel<IssueModel> {Data = data.Data, More = this.CreateMore(data)};
        }

        protected override List<IssueModel> FilterModel(List<IssueModel> model, IssuesFilterModel filter)
        {
            var order = (IssuesFilterModel.Order)Filter.OrderBy;
            return order == IssuesFilterModel.Order.Local_Id ? model.OrderBy(x => x.Number).ToList() : model.OrderBy(x => x.Title).ToList();
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

        protected override void SaveFilterAsDefault(IssuesFilterModel filter)
        {
            Application.Account.AddFilter(this, filter);
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

        private static IEnumerable<Tuple<string, string>> FieldToUrl(string name, object o)
        {
            var ret = new LinkedList<Tuple<string, string>>();
            foreach (var f in o.GetType().GetFields())
            {
                if ((bool)f.GetValue(o))
                {
                    //Special case for "on hold"
                    var objectName = f.Name.ToLower();
                    if (objectName.Equals("onhold"))
                        objectName = "on hold";
                    ret.AddLast(new Tuple<string, string>(name, objectName));
                }
            }
            return ret;
        }

        private List<IssueModel> GetData(int start = 0, int limit = 50, IEnumerable<Tuple<string, string>> additionalFilters = null)
        {
            LinkedList<Tuple<string, string>> filter = new LinkedList<Tuple<string, string>>();
            if (Filter != null)
            {
                if (Filter.Status != null && !Filter.Status.IsDefault())
                    foreach (var a in FieldToUrl("status", Filter.Status)) filter.AddLast(a);
                if (Filter.Kind != null && !Filter.Kind.IsDefault())
                    foreach (var a in FieldToUrl("kind", Filter.Kind)) filter.AddLast(a);
                if (Filter.Priority != null && !Filter.Priority.IsDefault())
                    foreach (var a in FieldToUrl("priority", Filter.Priority)) filter.AddLast(a);
                if (!string.IsNullOrEmpty(Filter.AssignedTo))
                {
                    if (Filter.AssignedTo.Equals("unassigned"))
                        filter.AddLast(new Tuple<string, string>("responsible", ""));
                    else
                        filter.AddLast(new Tuple<string, string>("responsible", Filter.AssignedTo));
                }
                if (!string.IsNullOrEmpty(Filter.ReportedBy))
                    filter.AddLast(new Tuple<string, string>("reported_by", Filter.ReportedBy));

                filter.AddLast(new Tuple<string, string>("sort", ((IssuesFilterModel.Order)Filter.OrderBy).ToString().ToLower()));
            }

            if (additionalFilters != null)
                foreach (var f in additionalFilters)
                    filter.AddLast(f);

            return new List<IssueModel>();
            //return Application.Client.Users[User].Repositories[Slug].Issues.GetIssues(start, limit, filter);
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

