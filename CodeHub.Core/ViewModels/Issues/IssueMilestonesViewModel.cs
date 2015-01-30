using System;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System.Collections.Generic;
using Octokit;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : ReactiveObject, ILoadableViewModel
    {
        private Milestone _selectedMilestone;

        public IReadOnlyReactiveList<IssueMilestoneItemViewModel> Milestones { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IssueMilestonesViewModel(
            Func<Task<IReadOnlyList<Milestone>>> loadMilestones,
            Func<Task<Issue>> loadIssue,
            Func<IssueUpdate, Task<Issue>> updateIssue
        )
        {
            var milestones = new ReactiveList<Milestone>();
            Milestones = milestones.CreateDerivedCollection(x =>
            {
                var vm = new IssueMilestoneItemViewModel(x);
                if (_selectedMilestone != null)
                    vm.IsSelected = x.Number == _selectedMilestone.Number;
                vm.GoToCommand.Subscribe(_ =>
                {
                    var milestone = vm.IsSelected ? (int?)vm.Number : null;
                    updateIssue(new IssueUpdate { Milestone = milestone }).ToBackground();
                });
                return vm;
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                _selectedMilestone = (await loadIssue()).Milestone;
                milestones.Reset(await loadMilestones());
            });
        }
    }
}

