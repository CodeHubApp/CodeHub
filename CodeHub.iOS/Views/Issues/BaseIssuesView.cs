using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.DialogElements;
using System;

namespace CodeHub.iOS.Views.Issues
{
    public abstract class BaseIssuesView : ViewModelCollectionDrivenDialogViewController
    {
        public new IBaseIssuesViewModel ViewModel
        {
            get { return (IBaseIssuesViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        protected BaseIssuesView()
        {
            Title = "Issues";
        }

        protected IssueElement CreateElement(Octokit.Issue x)
        {
            var weakVm = new WeakReference<IBaseIssuesViewModel>(ViewModel);
            var isPullRequest = x.PullRequest != null && !(string.IsNullOrEmpty(x.PullRequest.HtmlUrl));
            var assigned = x.Assignee != null ? x.Assignee.Login : "unassigned";
            var kind = isPullRequest ? "Pull" : "Issue";
            var commentString = x.Comments == 1 ? "1 comment" : x.Comments + " comments";
            var updatedAt = x.UpdatedAt ?? x.CreatedAt;
            var el = new IssueElement(x.Number.ToString(), x.Title, assigned, x.State.StringValue, commentString, kind, updatedAt);
            el.Tapped += () => weakVm.Get()?.GoToIssueCommand.Execute(x);
            return el;
        }
    }
}

