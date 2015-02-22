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

        public IReactiveList<Label> SelectedLabels { get; private set; }

        public IReactiveCommand SelectLabelsCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IssueLabelsViewModel(
            Func<Task<IReadOnlyList<Label>>> loadLabels,
            Func<Task<IReadOnlyList<Label>>> currentLabels,
            Func<IReadOnlyList<Label>, Task> updateIssue)
	    {
            var labels = new ReactiveList<Label>();
            SelectedLabels = new ReactiveList<Label>();
            Labels = labels.CreateDerivedCollection(x => 
            {
                var vm = new IssueLabelItemViewModel(x);
                vm.IsSelected = SelectedLabels.Any(y => string.Equals(y.Name, x.Name));
                vm.GoToCommand
                    .Select(_ => x)
                    .Where(y => vm.IsSelected)
                    .Where(y => SelectedLabels.All(l => l.Url != y.Url))
                    .Subscribe(SelectedLabels.Add);
                vm.GoToCommand
                    .Select(_ => x)
                    .Where(y => !vm.IsSelected)
                    .Select(y => SelectedLabels.Where(l => l.Url == y.Url).ToArray())
                    .Subscribe(SelectedLabels.RemoveAll);
                return vm;
            });

            SelectLabelsCommand = ReactiveCommand.CreateAsyncTask(t =>
	        {
                if (!SelectedLabels.All(_previouslySelectedLabels.Contains))
                    return updateIssue(new ReadOnlyCollection<Label>(SelectedLabels));
                return Task.FromResult(0);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var currentlySelectedLabels = (await currentLabels()) ?? Enumerable.Empty<Label>();
                SelectedLabels.Reset(currentlySelectedLabels);

                _previouslySelectedLabels.Clear();
                foreach (var l in currentlySelectedLabels)
                    _previouslySelectedLabels.Add(l);

                labels.Reset(await loadLabels());
            });
	    }
    }
}

