using System;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using Octokit;
using System.Collections.Generic;

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

        public IssueAssigneeViewModel(
            Func<Task<IReadOnlyList<User>>> loadAssignees,
            Func<Task<Issue>> loadIssue,
            Func<IssueUpdate, Task<Issue>> updateIssue
        )
        {
            var derivedFunc = new Func<User, IssueAssigneeItemViewModel>(x =>
            {
                var vm = new IssueAssigneeItemViewModel(x);
                if (_selectedUser != null)
                    vm.IsSelected = x.Id == _selectedUser.Id;

                vm.GoToCommand.Subscribe(_ =>
                {
                    var assigneeName = vm.IsSelected ? vm.Name : null;
                    updateIssue(new IssueUpdate { Assignee = assigneeName }).ToBackground();
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
                _selectedUser = (await loadIssue()).Assignee;
                assignees.Reset(await loadAssignees());
            });
        }
    }
}

