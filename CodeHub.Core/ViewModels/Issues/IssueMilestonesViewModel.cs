using System;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System.Collections.Generic;
using Octokit;
using System.Reactive.Linq;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : ReactiveObject, ILoadableViewModel
    {
        private Milestone _selected;
        public Milestone Selected
        {
            get { return _selected; }
            set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        public IReadOnlyReactiveList<IssueMilestoneItemViewModel> Milestones { get; private set; }

        public IReactiveCommand<Milestone> SelectMilestoneCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> DismissCommand { get; private set; }

        public IssueMilestonesViewModel(Func<Task<IReadOnlyList<Milestone>>> loadMilestones)
        {
            DismissCommand = ReactiveCommand.Create();

            var milestones = new ReactiveList<Milestone>();
            Milestones = milestones.CreateDerivedCollection(x => {
                var vm = new IssueMilestoneItemViewModel(x);
                vm.IsSelected = x.Number == Selected?.Number;
                vm.GoToCommand
                    .Select(_ => vm.IsSelected ? x : null)
                    .Subscribe(milestone =>  {
                        foreach (var a in Milestones.Where(y => y != vm))
                            a.IsSelected = false;
                        Selected = milestone;
                        DismissCommand.ExecuteIfCan();
                    });
                return vm;
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                milestones.Reset(await loadMilestones());
            });
        }
    }
}

