using System;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Factories;
using CodeHub.Core.Filters;
using System.Linq;
using System.Reactive.Linq;
using Humanizer;
using System.Collections.Generic;
using CodeHub.Core.Services;
using Octokit;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace CodeHub.Core.ViewModels.Issues
{
    public class RepositoryIssuesFilterViewModel : BaseViewModel
    {
        private readonly Lazy<Task<IReadOnlyList<User>>> _assigneesCache;
        private readonly Lazy<Task<IReadOnlyList<Milestone>>> _milestonesCache;
        private readonly Lazy<Task<IReadOnlyList<Label>>> _labelsCache;

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public IReactiveCommand<IssuesFilterModel> SaveCommand { get; private set; }

        public IReactiveCommand<object> DismissCommand { get; private set; }

        public IReactiveCommand<object> SelectAssigneeCommand { get; private set; }

        public IReactiveCommand<object> SelectMilestoneCommand { get; private set; }

        public IReactiveCommand<object> SelectLabelsCommand { get; private set; }

        public IReactiveCommand<Unit> SelectStateCommand { get; private set; }

        public IReactiveCommand<Unit> SelectSortCommand { get; private set; }

        private IReadOnlyList<Label> _labels;
        public IReadOnlyList<Label> Labels
        {
            get { return _labels; }
            private set { this.RaiseAndSetIfChanged(ref _labels, value); }
        }

        private Milestone _milestone;
        public Milestone Milestone
        {
            get { return _milestone; }
            private set { this.RaiseAndSetIfChanged(ref _milestone, value); }
        }

        private User _assignee;
        public User Assignee
        {
            get { return _assignee; }
            private set { this.RaiseAndSetIfChanged(ref _assignee, value); }
        }

        private string _creator;
        public string Creator
        {
            get { return _creator; }
            set { this.RaiseAndSetIfChanged(ref _creator, value); }
        }

        private string _mentioned;
        public string Mentioned
        {
            get { return _mentioned; }
            set { this.RaiseAndSetIfChanged(ref _mentioned, value); }
        }

        private readonly ObservableAsPropertyHelper<string> _assigneeString;
        public string AssigneeString 
        {
            get { return _assigneeString.Value; }
        }

        private readonly ObservableAsPropertyHelper<string> _milestoneString;
        public string MilestoneString 
        {
            get { return _milestoneString.Value; }
        }

        private readonly ObservableAsPropertyHelper<string> _labelsString;
        public string LabelsString 
        {
            get { return _labelsString.Value; }
        }

        private bool _ascending;
        public bool Ascending
        {
            get { return _ascending; }
            set { this.RaiseAndSetIfChanged(ref _ascending, value); }
        }

        private CodeHub.Core.Filters.IssueSort _sortType;
        public CodeHub.Core.Filters.IssueSort SortType
        {
            get { return _sortType; }
            private set { this.RaiseAndSetIfChanged(ref _sortType, value); }
        }

        private IssueState _state;
        public IssueState State
        {
            get { return _state; }
            private set { this.RaiseAndSetIfChanged(ref _state, value); }
        }

        public RepositoryIssuesFilterViewModel(ISessionService sessionService, IActionMenuFactory actionMenu)
        {
            Title = "Filter";
            State = IssueState.Open;
            SortType = CodeHub.Core.Filters.IssueSort.None;

            SaveCommand = ReactiveCommand.CreateAsyncTask(_ => {
                var model = new IssuesFilterModel(Assignee, Creator, Mentioned, Labels, Milestone, State, SortType, Ascending);
                return Task.FromResult(model);      
            });
            SaveCommand.Subscribe(_ => Dismiss());
            DismissCommand = ReactiveCommand.Create().WithSubscription(_ => Dismiss());

            _assigneesCache = new Lazy<Task<IReadOnlyList<User>>>(() => 
                sessionService.GitHubClient.Issue.Assignee.GetAllForRepository(RepositoryOwner, RepositoryName));
            _milestonesCache = new Lazy<Task<IReadOnlyList<Milestone>>>(() => 
                sessionService.GitHubClient.Issue.Milestone.GetAllForRepository(RepositoryOwner, RepositoryName));
            _labelsCache = new Lazy<Task<IReadOnlyList<Label>>>(() => 
                sessionService.GitHubClient.Issue.Labels.GetAllForRepository(RepositoryOwner, RepositoryName));

            this.WhenAnyValue(x => x.Milestone)
                .Select(x => x?.Title)
                .ToProperty(this, x => x.MilestoneString, out _milestoneString);

            this.WhenAnyValue(x => x.Assignee)
                .Select(x => x?.Login)
                .ToProperty(this, x => x.AssigneeString, out _assigneeString);

            this.WhenAnyValue(x => x.Labels)
                .Select(x => x ?? new List<Label>())
                .Select(x => string.Join(", ", x.Select(y => y.Name)))
                .ToProperty(this, x => x.LabelsString, out _labelsString);
            
            SelectAssigneeCommand = ReactiveCommand.Create();
            SelectMilestoneCommand = ReactiveCommand.Create();
            SelectLabelsCommand = ReactiveCommand.Create();

            SelectStateCommand = ReactiveCommand.CreateAsyncTask(async sender => {
                var options = Enum.GetValues(typeof(IssueState)).Cast<IssueState>().ToList();
                var picker = actionMenu.CreatePicker();
                foreach (var option in options)
                    picker.Options.Add(option.Humanize());
                picker.SelectedOption = options.IndexOf(State);
                var ret = await picker.Show(sender);
                State = options[ret];
            });

            SelectSortCommand = ReactiveCommand.CreateAsyncTask(async sender => {
                var options = Enum.GetValues(typeof(CodeHub.Core.Filters.IssueSort)).Cast<CodeHub.Core.Filters.IssueSort>().ToList();
                var picker = actionMenu.CreatePicker();
                foreach (var option in options)
                    picker.Options.Add(option.Humanize());
                picker.SelectedOption = options.IndexOf(SortType);
                var ret = await picker.Show(sender);
                SortType = options[ret];
            });
        }

        public RepositoryIssuesFilterViewModel Init(string repositoryOwner, string repositoryName, IssuesFilterModel model)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            Assignee = model.Assignee;
            Creator = model.Creator;
            Milestone = model.Milestone;
            Labels = model.Labels;
            Mentioned = model.Mentioned;
            SortType = model.SortType;
            State = model.IssueState;
            Ascending = model.Ascending;
            return this;
        }

        public IssueAssigneeViewModel CreateAssigneeViewModel()
        {
            var vm = new IssueAssigneeViewModel(
                () => _assigneesCache.Value,
                () => Task.FromResult(Assignee),
                x => Task.FromResult(Assignee = x));
            vm.LoadCommand.ExecuteIfCan();
            return vm;
        }

        public IssueMilestonesViewModel CreateMilestonesViewModel()
        {
            var vm = new IssueMilestonesViewModel(
                () => _milestonesCache.Value,
                () => Task.FromResult(Milestone),
                x => Task.FromResult(Milestone = x));
            vm.LoadCommand.ExecuteIfCan();
            return vm;
        }

        public IssueLabelsViewModel CreateLabelsViewModel()
        {
            var vm = new IssueLabelsViewModel(
                () => _labelsCache.Value,
                () => Task.FromResult(Labels),
                x => Task.FromResult(Labels = new ReadOnlyCollection<Label>(x.ToList())));
            vm.LoadCommand.ExecuteIfCan();
            return vm;
        }

        private static string IssueAssigneeFilterToString(IssueAssigneeFilter filter)
        {
            if (filter == null) return null;
            return filter.NoAssignedUser ? "No Assigned User" : filter.AssignedUser;
        }

    }
}

