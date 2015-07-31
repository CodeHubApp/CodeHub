using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Octokit;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : ReactiveObject, ILoadableViewModel
    {
        public IReadOnlyReactiveList<IssueLabelItemViewModel> Labels { get; }

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReactiveCommand<Unit> SaveCommand { get; }

        public IssueLabelsViewModel(
            Func<Task<IReadOnlyList<Label>>> loadAllLabelsFunc,
            Func<Task<IReadOnlyList<Label>>> loadSelectedFunc,
            Func<IEnumerable<Label>, Task> saveLabelsFunc)
	    {
            var labels = new ReactiveList<Label>();
            var selected = new ReactiveList<Label>();
            Labels = labels.CreateDerivedCollection(x => 
            {
                var vm = new IssueLabelItemViewModel(x);
                vm.IsSelected = selected.Any(y => y.Url == x.Url);
                return vm;
            });

            SaveCommand = ReactiveCommand.CreateAsyncTask(_ =>
            {
                var currentlySelected = Labels.Where(x => x.IsSelected).ToList();
                var selectedLabelsUrl = currentlySelected.Select(x => x.Label.Url).ToArray();
                var prevSelectedLabelsUrl = selected.Select(x => x.Url).ToArray();
                var intersect = selectedLabelsUrl.Intersect(prevSelectedLabelsUrl).ToArray();
                var different = selectedLabelsUrl.Length != prevSelectedLabelsUrl.Length || intersect.Length != selectedLabelsUrl.Length;
                return different ? saveLabelsFunc(currentlySelected.Select(x => x.Label)) : Task.FromResult(0);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                selected.Clear();
                selected.AddRange((await loadSelectedFunc()) ?? Enumerable.Empty<Label>());
                labels.Reset(await loadAllLabelsFunc());
            });
	    }
    }
}

