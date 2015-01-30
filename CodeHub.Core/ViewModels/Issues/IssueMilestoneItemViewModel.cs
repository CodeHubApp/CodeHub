using System;
using ReactiveUI;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestoneItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Title { get; private set; }

        public int OpenIssues { get; private set; }

        public int ClosedIssues { get; private set; }

        public DateTimeOffset? DueDate { get; private set; }

        public int Number { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        public IssueMilestoneItemViewModel(Octokit.Milestone milestone)
        {
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

