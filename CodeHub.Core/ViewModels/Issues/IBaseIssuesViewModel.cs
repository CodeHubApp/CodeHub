using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public interface IBaseIssuesViewModel : IBaseViewModel
    {
        IReadOnlyReactiveList<IssueItemViewModel> Issues { get; }

        IReactiveCommand GoToIssueCommand { get; }

        string SearchKeyword { get; set; }
    }
}
