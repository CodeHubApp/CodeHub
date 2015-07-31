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
        private User _previouslySelected;

        private User _selected;
        public User Selected
        {
            get { return _selected; }
            private set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReadOnlyReactiveList<IssueAssigneeItemViewModel> Assignees { get; }

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReactiveCommand<Unit> SaveCommand { get; }

        public IReactiveCommand<object> DismissCommand { get; }

        public IssueAssigneeViewModel(
            Func<Task<IReadOnlyList<User>>> loadAssignees,
            Func<Task<User>> loadSelectedFunc,
            Func<User, Task> saveFunc)
        {
            var assignees = new ReactiveList<IssueAssigneeItemViewModel>();
            Assignees = assignees.CreateDerivedCollection(
                x => x,
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            this.WhenAnyValue(x => x.Selected)
                .Subscribe(x => {
                    foreach (var a in Assignees)
                        a.IsSelected = string.Equals(a.User.Login, x?.Login);
                });

            DismissCommand = ReactiveCommand.Create();

            SaveCommand = ReactiveCommand.CreateAsyncTask(_ => {
                DismissCommand.ExecuteIfCan();
                return Selected != _previouslySelected ? saveFunc(_selected) : Task.FromResult(0);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                _previouslySelected = Selected = await loadSelectedFunc();
                assignees.Reset((await loadAssignees()).Select(CreateItemViewModel));
            });
        }

        private IssueAssigneeItemViewModel CreateItemViewModel(User x)
        {
            var vm = new IssueAssigneeItemViewModel(x);
            vm.IsSelected = x.Id == _selected?.Id;
            vm.GoToCommand.Subscribe(_ => {
                Selected = vm.IsSelected ? x : null;
                SaveCommand.ExecuteIfCan();
            });
            return vm;
        }
    }
}

