using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public interface IBaseIssuesViewModel
    {
        IReadOnlyReactiveList<IssueModel> Issues { get; }

        IReactiveCommand GoToIssueCommand { get; }
    }
}
