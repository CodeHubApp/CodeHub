using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using Octokit;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : ReactiveObject, ILoadableViewModel
    {
        private readonly IList<Label> _selectedLabels = new List<Label>();
        private Issue _issue;

        public IReadOnlyReactiveList<IssueLabelItemViewModel> Labels { get; private set; }

        public IReactiveCommand SelectLabelsCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IssueLabelsViewModel(
            Func<Task<IReadOnlyList<Label>>> loadLabels,
            Func<Task<Issue>> loadIssue,
            Func<IssueUpdate, Task<Issue>> updateIssue, 
            IGraphicService graphicService)
	    {
            var labels = new ReactiveList<Label>();

            Labels = labels.CreateDerivedCollection(x => 
            {
                var vm = new IssueLabelItemViewModel(graphicService, x);
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
                if (!_selectedLabels.All(_issue.Labels.Contains))
                    return updateIssue(new IssueUpdate { Labels = _selectedLabels.Select(x => x.Name).ToList() });
                return Task.FromResult(0);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                _issue = await loadIssue();
                _selectedLabels.Clear();
                foreach (var l in _issue.Labels)
                    _selectedLabels.Add(l);
                labels.Reset(await loadLabels());
            });
	    }
    }
}

