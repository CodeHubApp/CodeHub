using CodeFramework.Elements;
using CodeFramework.ViewControllers;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Issues;
using UIKit;

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

            el.Tapped += () => {
                //Make sure the first responder is gone.
                View.EndEditing(true);
				ViewModel.GoToIssueCommand.Execute(x);
            };

            return el;
        }
    }
}

