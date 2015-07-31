using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestoneItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Title { get; }

        public int OpenIssues { get; }

        public int ClosedIssues { get; }

        public DateTimeOffset? DueDate { get; }

        public int Number { get; }

        public IReactiveCommand<object> GoToCommand { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        internal Octokit.Milestone Milestone { get; }

        public IssueMilestoneItemViewModel(Octokit.Milestone milestone)
        {
            Milestone = milestone;
            Number = milestone.Number;
            Title = milestone.Title;
            OpenIssues = milestone.OpenIssues;
            ClosedIssues = milestone.ClosedIssues;
            DueDate = milestone.DueOn;

            GoToCommand = ReactiveCommand.Create()
                .WithSubscription(_ => IsSelected = !IsSelected);
        }
    }
}

