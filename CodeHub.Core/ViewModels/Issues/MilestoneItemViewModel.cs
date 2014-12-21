using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public class MilestoneItemViewModel : ReactiveObject
    {
        public string Title { get; private set; }

        public int OpenIssues { get; private set; }

        public int ClosedIssues { get; private set; }

        public DateTimeOffset? DueDate { get; private set; }

        public MilestoneItemViewModel(string title, int openIssues, int closedIssues, DateTimeOffset? dueDate)
        {
            Title = title;
            OpenIssues = openIssues;
            ClosedIssues = closedIssues;
            DueDate = dueDate;
        }
    }
}

