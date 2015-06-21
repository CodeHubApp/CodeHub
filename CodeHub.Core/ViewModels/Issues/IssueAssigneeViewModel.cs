using System;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using Octokit;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAssigneeViewModel : ReactiveObject, ILoadableViewModel, IProvidesSearchKeyword
    {
        private User _selected;
        public User Selected
        {
            get { return _selected; }
            set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReadOnlyReactiveList<IssueAssigneeItemViewModel> Assignees { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> DismissCommand { get; private set; }

        public IssueAssigneeViewModel(
            Func<Task<IReadOnlyList<User>>> loadAssignees)
        {
            DismissCommand = ReactiveCommand.Create();

            var derivedFunc = new Func<User, IssueAssigneeItemViewModel>(x => {
                var vm = new IssueAssigneeItemViewModel(x);
                vm.IsSelected = x.Id == Selected?.Id;

                vm.GoToCommand
                    .Select(_ => vm.IsSelected ? x : null)
                    .Subscribe(user =>  {
                        foreach (var a in Assignees.Where(y => y != vm))
                            a.IsSelected = false;
                        Selected = user;
                        DismissCommand.ExecuteIfCan();
                    });
                return vm;
            });

            var assignees = new ReactiveList<User>();
            Assignees = assignees.CreateDerivedCollection(
                derivedFunc,
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                assignees.Reset(await loadAssignees());
            });
        }
    }
}

