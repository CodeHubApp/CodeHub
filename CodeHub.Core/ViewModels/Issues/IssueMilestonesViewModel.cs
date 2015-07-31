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
        private Milestone _previouslySelected;

        private Milestone _selected;
        public Milestone Selected
        {
            get { return _selected; }
            private set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        public IReadOnlyReactiveList<IssueMilestoneItemViewModel> Milestones { get; }

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReactiveCommand<Unit> SaveCommand { get; }

        public IReactiveCommand<object> DismissCommand { get; }

        public IssueMilestonesViewModel(
            Func<Task<IReadOnlyList<Milestone>>> loadMilestones,
            Func<Task<Milestone>> loadSelectedFunc,
            Func<Milestone, Task> saveFunc)
        {
            var milestones = new ReactiveList<Milestone>();
            Milestones = milestones.CreateDerivedCollection(x => CreateItemViewModel(x));

            this.WhenAnyValue(x => x.Selected)
                .Subscribe(x => {
                    foreach (var a in Milestones)
                        a.IsSelected = a.Number == x?.Number;
                });

            DismissCommand = ReactiveCommand.Create();

            SaveCommand = ReactiveCommand.CreateAsyncTask(_ => {
                DismissCommand.ExecuteIfCan();
                return _selected != _previouslySelected ? saveFunc(_selected) : Task.FromResult(0);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                _previouslySelected = Selected = await loadSelectedFunc();
                milestones.Reset(await loadMilestones());
            });
        }

        private IssueMilestoneItemViewModel CreateItemViewModel(Milestone x)
        {
            var vm = new IssueMilestoneItemViewModel(x);
            vm.IsSelected = x.Number == Selected?.Number;
            vm.GoToCommand.Subscribe(_ => {
                Selected = vm.IsSelected ? x : null;
                SaveCommand.ExecuteIfCan();
            });
            return vm;
        }
    }
}

