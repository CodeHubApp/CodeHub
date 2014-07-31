using System.Globalization;
using CodeFramework.Elements;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.Core.ViewModels;
using ReactiveUI;

namespace CodeHub.iOS.Views.Issues
{
    public abstract class BaseIssuesView<TViewModel> : ViewModelCollectionViewController<TViewModel> where TViewModel : class, IBaseViewModel, IBaseIssuesViewModel
    {
        protected BaseIssuesView()
            : base(unevenRows: true)
        {
            Title = "Issues";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.BindList(ViewModel.Issues, CreateElement);
        }

        protected Element CreateElement(IssueModel x)
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

