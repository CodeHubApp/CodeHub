using System;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System.Collections.Generic;
using Octokit;
using System.Reactive.Linq;
using System.Linq;
using System.Diagnostics;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : ReactiveObject, ILoadableViewModel
    {
        private Milestone _selectedMilestone;

        public IReadOnlyReactiveList<IssueMilestoneItemViewModel> Milestones { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<Milestone> SelectMilestoneCommand { get; private set; }

        public IReactiveCommand<object> DismissCommand { get; private set; }

        public IssueMilestonesViewModel(
            Func<Task<IReadOnlyList<Milestone>>> loadMilestones,
            Func<Task<Milestone>> currentMilestone,
            Func<Milestone, Task> updateIssue
        )
        {
            DismissCommand = ReactiveCommand.Create();

            var milestones = new ReactiveList<Milestone>();
            Milestones = milestones.CreateDerivedCollection(x =>
            {
                var vm = new IssueMilestoneItemViewModel(x);
                if (_selectedMilestone != null)
                    vm.IsSelected = x.Number == _selectedMilestone.Number;
                vm.GoToCommand
                    .Select(_ => vm.IsSelected ? x : null)
                    .Subscribe(milestone => 
                    {
                        foreach (var a in Milestones.Where(y => y != vm))
                            a.IsSelected = false;
                        updateIssue(milestone).ToBackground();
                        DismissCommand.ExecuteIfCan();
                    });
                return vm;
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                _selectedMilestone = (await currentMilestone());
                milestones.Reset(await loadMilestones());
            });
        }
    }
}

