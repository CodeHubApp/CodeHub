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
        private User _selectedUser;

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReadOnlyReactiveList<IssueAssigneeItemViewModel> Assignees { get; private set; }

        public IReactiveCommand<object> DismissCommand { get; private set; }

        public IssueAssigneeViewModel(
            Func<Task<IReadOnlyList<User>>> loadAssignees,
            Func<Task<User>> loadCurrentlyAssigned,
            Func<User, Task> updateIssue
        )
        {
            DismissCommand = ReactiveCommand.Create();

            var derivedFunc = new Func<User, IssueAssigneeItemViewModel>(x =>
            {
                var vm = new IssueAssigneeItemViewModel(x);
                if (_selectedUser != null)
                    vm.IsSelected = x.Id == _selectedUser.Id;

                vm.GoToCommand
                    .Select(_ => vm.IsSelected ? x : null)
                    .Subscribe(user => 
                    {
                        foreach (var a in Assignees.Where(y => y != vm))
                            a.IsSelected = false;
                        updateIssue(user).ToBackground();
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
                _selectedUser = (await loadCurrentlyAssigned());
                assignees.Reset(await loadAssignees());
            });
        }
    }
}

