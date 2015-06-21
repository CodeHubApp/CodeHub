using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Octokit;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : ReactiveObject, ILoadableViewModel
    {
        private readonly IList<Label> _previouslySelectedLabels = new List<Label>();

        public IReadOnlyReactiveList<IssueLabelItemViewModel> Labels { get; private set; }

        private IReadOnlyList<Label> _selected;
        public IReadOnlyList<Label> Selected
        {
            get { return _selected; }
            set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        public IReactiveCommand SelectLabelsCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IssueLabelsViewModel(Func<Task<IReadOnlyList<Label>>> loadLabels)
	    {
            var labels = new ReactiveList<Label>();
            Selected = new ReactiveList<Label>();
            Labels = labels.CreateDerivedCollection(x => 
            {
                var vm = new IssueLabelItemViewModel(x);
                vm.IsSelected = Selected.Any(y => string.Equals(y.Name, x.Name));
//                vm.GoToCommand
//                    .Select(_ => x)
//                    .Where(y => vm.IsSelected)
//                    .Where(y => Selected.All(l => l.Url != y.Url))
//                    .Subscribe(Selected.Add);
//                vm.GoToCommand
//                    .Select(_ => x)
//                    .Where(y => !vm.IsSelected)
//                    .Select(y => Selected.Where(l => l.Url == y.Url).ToArray())
//                    .Subscribe(Selected.RemoveAll);
                return vm;
            });

            SelectLabelsCommand = ReactiveCommand.CreateAsyncTask(t => {
                var selectedLabelsUrl = Selected.Select(x => x.Url).ToArray();
                var prevSelectedLabelsUrl = _previouslySelectedLabels.Select(x => x.Url).ToArray();
                var intersect = selectedLabelsUrl.Intersect(prevSelectedLabelsUrl).ToArray();
                var different = selectedLabelsUrl.Length != prevSelectedLabelsUrl.Length || intersect.Length != selectedLabelsUrl.Length;
                return Task.FromResult(0); //different ? updateIssue(new ReadOnlyCollection<Label>(Selected)) : Task.FromResult(0);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                labels.Reset(await loadLabels());
            });
	    }
    }
}

