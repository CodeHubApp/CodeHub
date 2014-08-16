using System;
using System.Globalization;
using CodeFramework.Elements;
using CodeHub.Core.ViewModels.Issues;
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

            this.WhenActivated(d =>
            {
                d(SearchTextChanging.Subscribe(x => ViewModel.SearchKeyword = x));
            });
        }

        protected Element CreateElement(IssueItemViewModel x)
        {
            var assigned = x.Issue.Assignee != null ? x.Issue.Assignee.Login : "unassigned";
            var kind = x.IsPullRequest ? "Pull" : "Issue";
            var commentString = x.Issue.Comments == 1 ? "1 comment" : x.Issue.Comments + " comments";
            var el = new IssueElement(x.Issue.Number.ToString(CultureInfo.InvariantCulture), x.Issue.Title, assigned, x.Issue.State, commentString, kind, x.Issue.UpdatedAt);
            el.Tapped += () => ViewModel.GoToIssueCommand.ExecuteIfCan(x);
            return el;
        }
    }
}

