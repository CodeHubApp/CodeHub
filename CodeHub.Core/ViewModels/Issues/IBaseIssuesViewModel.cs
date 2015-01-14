using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.Issues
{
    public interface IBaseIssuesViewModel : IBaseViewModel, IProvidesSearchKeyword
    {
        IList<IssueGroupViewModel> GroupedIssues { get; }
    }
}
