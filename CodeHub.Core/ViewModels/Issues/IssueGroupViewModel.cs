using ReactiveUI;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueGroupViewModel
    {
        public string Name { get; }

        public IReadOnlyReactiveList<IssueItemViewModel> Issues { get; }

        public IssueGroupViewModel(string name, IEnumerable<IssueItemViewModel> issues)
        {
            Issues = new ReactiveList<IssueItemViewModel>(issues);
            Name = name;
        }
    }
}

