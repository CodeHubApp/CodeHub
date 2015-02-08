using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using Octokit;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : ReactiveObject, ILoadableViewModel
    {
        private readonly IList<Label> _previouslySelectedLabels = new List<Label>();
        private readonly IList<Label> _selectedLabels = new List<Label>();

        public IReadOnlyReactiveList<IssueLabelItemViewModel> Labels { get; private set; }

        public IReactiveCommand SelectLabelsCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IssueLabelsViewModel(
            Func<Task<IReadOnlyList<Label>>> loadLabels,
            Func<Task<IReadOnlyList<Label>>> currentLabels,
            Func<IReadOnlyList<Label>, Task> updateIssue)
	    {
            var labels = new ReactiveList<Label>();

            Labels = labels.CreateDerivedCollection(x => 
            {
                var vm = new IssueLabelItemViewModel(x);
                vm.IsSelected = _selectedLabels.Any(y => string.Equals(y.Name, x.Name));
                vm.GoToCommand
                    .Select(_ => x)
                    .Where(y => vm.IsSelected && !_selectedLabels.Contains(y))
                    .Subscribe(_selectedLabels.Add);
                vm.GoToCommand
                    .Select(_ => x)
                    .Where(y => !vm.IsSelected)
                    .Subscribe(y => _selectedLabels.Remove(y));
                return vm;
            });

            SelectLabelsCommand = ReactiveCommand.CreateAsyncTask(t =>
	        {
                if (!_selectedLabels.All(_previouslySelectedLabels.Contains))
                    return updateIssue(new ReadOnlyCollection<Label>(_selectedLabels));
                return Task.FromResult(0);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                _previouslySelectedLabels.Clear();
                _selectedLabels.Clear();

                var currentlySelectedLabels = (await currentLabels()) ?? Enumerable.Empty<Label>();
                foreach (var l in currentlySelectedLabels)
                {
                    _previouslySelectedLabels.Add(l);
                    _selectedLabels.Add(l);
                }

                labels.Reset(await loadLabels());
            });
	    }
    }
}

