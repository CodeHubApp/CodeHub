using ReactiveUI;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueGroupViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public IReadOnlyReactiveList<IssueItemViewModel> Issues { get; private set; }

        public IssueGroupViewModel(string name, IEnumerable<IssueItemViewModel> issues)
        {
            Issues = new ReactiveList<IssueItemViewModel>(issues);
            Name = name;
        }
    }
}

