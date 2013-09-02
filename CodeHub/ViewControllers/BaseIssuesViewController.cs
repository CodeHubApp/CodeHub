using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeFramework.Elements;

namespace CodeHub.ViewControllers
{
    public abstract class BaseIssuesViewController : BaseListControllerDrivenViewController, IListView<IssueModel>
    {
        protected BaseIssuesViewController()
        {
            Root.UnevenRows = true;
            Title = "Issues".t();
            SearchPlaceholder = "Search Issues".t();
        }

        public void Render(ListModel<IssueModel> model)
        {
            RenderList(model, x => {
                var isPullRequest = x.PullRequest != null && !(string.IsNullOrEmpty(x.PullRequest.HtmlUrl));
                var assigned = x.Assignee != null ? x.Assignee.Login : "unassigned";
                var kind = isPullRequest ? "Pull" : "Issue";
                var commentString = x.Comments == 1 ? "1 comment".t() : x.Comments + " comments".t();
                var el = new IssueElement(x.Number.ToString(), x.Title, assigned, x.State, commentString, kind, x.UpdatedAt);
                el.Tag = x;

                if (isPullRequest)
                {
                    el.Tapped += () => {
                        //Make sure the first responder is gone.
                        View.EndEditing(true);
                        var s1 = x.Url.Substring(x.Url.IndexOf("/repos/") + 7);
                        var repoId = new CodeHub.Utils.RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues")));
                        var info = new PullRequestViewController(repoId.Owner, repoId.Name, x.Number);
                        //info.Controller.ModelChanged = newModel => ChildChangedModel(newModel, x);
                        NavigationController.PushViewController(info, true);
                    };
                }
                else
                {
                    el.Tapped += () => {
                        //Make sure the first responder is gone.
                        View.EndEditing(true);
                        var s1 = x.Url.Substring(x.Url.IndexOf("/repos/") + 7);
                        var repoId = new CodeHub.Utils.RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues")));
                        var info = new IssueViewController(repoId.Owner, repoId.Name, x.Number);
                        info.Controller.ModelChanged = newModel => ChildChangedModel(newModel, x);
                        NavigationController.PushViewController(info, true);
                    };
                }
                return el;
            });
        }

        protected abstract void ChildChangedModel(IssueModel changedModel, IssueModel oldModel);
    }
}

