using System;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System.Collections.Generic;
using Octokit;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : ReactiveObject, ILoadableViewModel
    {
        private Milestone _selectedMilestone;

        public IReadOnlyReactiveList<IssueMilestoneItemViewModel> Milestones { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IssueMilestonesViewModel(
            Func<Task<IReadOnlyList<Milestone>>> loadMilestones,
            Func<Task<Milestone>> currentMilestone,
            Func<Milestone, Task> updateIssue
        )
        {
            var milestones = new ReactiveList<Milestone>();
            Milestones = milestones.CreateDerivedCollection(x =>
            {
                var vm = new IssueMilestoneItemViewModel(x);
                if (_selectedMilestone != null)
                    vm.IsSelected = x.Number == _selectedMilestone.Number;
                vm.GoToCommand
                    .Select(_ => vm.IsSelected ? x : null)
                    .Subscribe(milestone => updateIssue(milestone).ToBackground());
                return vm;
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                _selectedMilestone = (await currentMilestone());
                milestones.Reset(await loadMilestones());
            });
        }
    }
}

