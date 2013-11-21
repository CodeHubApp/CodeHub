using CodeFramework.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.Utils;
using CodeHub.iOS.Views.PullRequests;
using GitHubSharp.Models;

namespace CodeHub.iOS.Views.Issues
{
    public abstract class BaseIssuesView : ViewModelCollectionDrivenViewController
    {
        protected BaseIssuesView()
        {
            Root.UnevenRows = true;
            Title = "Issues".t();
        }

        protected MonoTouch.Dialog.Element CreateElement(IssueModel x)
        {
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
                    var repoId = new RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues")));
//                    var info = new PullRequestView(repoId.Owner, repoId.Name, x.Number);
//                    //info.Controller.ModelChanged = newModel => ChildChangedModel(newModel, x);
//                    NavigationController.PushViewController(info, true);
                };
            }
            else
            {
                el.Tapped += () => {
                    //Make sure the first responder is gone.
                    View.EndEditing(true);
                    var s1 = x.Url.Substring(x.Url.IndexOf("/repos/") + 7);
                    var repoId = new RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues")));
//                    var info = new IssueView(repoId.Owner, repoId.Name, x.Number);
//                    //info.ViewModel.ModelChanged = newModel => ChildChangedModel(newModel, x);
//                    NavigationController.PushViewController(info, true);
                };
            }
            return el;
        }
    }
}

