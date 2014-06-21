using System.Globalization;
using CodeFramework.Elements;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.iOS.Views.Issues
{
    public abstract class BaseIssuesView<TViewModel> : ViewModelCollectionView<TViewModel> where TViewModel : ReactiveObject, IBaseIssuesViewModel
    {
        protected BaseIssuesView()
        {
            Root.UnevenRows = true;
            Title = "Issues";
        }

        protected MonoTouch.Dialog.Element CreateElement(IssueModel x)
        {
			var isPullRequest = x.PullRequest != null && !(string.IsNullOrEmpty(x.PullRequest.HtmlUrl));
            var assigned = x.Assignee != null ? x.Assignee.Login : "unassigned";
            var kind = isPullRequest ? "Pull" : "Issue";
            var commentString = x.Comments == 1 ? "1 comment" : x.Comments + " comments";
            var el = new IssueElement(x.Number.ToString(CultureInfo.InvariantCulture), x.Title, assigned, x.State, commentString, kind, x.UpdatedAt);
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

