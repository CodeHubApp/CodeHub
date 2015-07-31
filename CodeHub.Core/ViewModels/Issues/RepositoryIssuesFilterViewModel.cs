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
        private readonly Lazy<Task<User>> _userGet;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IReactiveCommand<object> SaveCommand { get; private set; }

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

            _userGet = new Lazy<Task<User>>(sessionService.GitHubClient.User.Current);

            SaveCommand = ReactiveCommand.Create().WithSubscription(_ => Dismiss());
//
//            this.WhenAnyValue(x => x.Milestones.Selected)
//                .Select(x => x?.Title)
//                .ToProperty(this, x => x.MilestoneString, out _milestoneString);
//
//            this.WhenAnyValue(x => x.Assignees.Selected)
//                .Select(x => x?.Login)
//                .ToProperty(this, x => x.AssigneeString, out _assigneeString);
//
//            this.WhenAnyValue(x => x.Labels.Selected)
//                .Select(x => x ?? new List<Label>())
//                .Select(x => string.Join(",", x.Select(y => y.Name)))
//                .ToProperty(this, x => x.LabelsString, out _labelsString);

            var getAssignees = new Lazy<Task<IReadOnlyList<User>>>(() => sessionService.GitHubClient.Issue.Assignee.GetAllForRepository(RepositoryOwner, RepositoryName));
            var getMilestones = new Lazy<Task<IReadOnlyList<Milestone>>>(() => sessionService.GitHubClient.Issue.Milestone.GetAllForRepository(RepositoryOwner, RepositoryName));
            var getLables = new Lazy<Task<IReadOnlyList<Label>>>(() => sessionService.GitHubClient.Issue.Labels.GetAllForRepository(RepositoryOwner, RepositoryName));

//            Assignees = new IssueAssigneeViewModel(() => getAssignees.Value);
//            Milestones = new IssueMilestonesViewModel(() => getMilestones.Value);
//            Labels = new IssueLabelsViewModel(() => getLables.Value);

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

        public async Task SetDefault(bool open, string assignee = null)
        {
            Creator = null;
            Mentioned = null;
            //Assignees.Selected = assignee == null ? null : (await _userGet.Value);
//            Milestones.Selected = null;
//            Labels.Selected = null;
            Ascending = false;
            SortType = CodeHub.Core.Filters.IssueSort.None;
            State = open ? IssueState.Open : IssueState.Closed;
            SaveCommand.ExecuteIfCan();
        }

        private static string IssueAssigneeFilterToString(IssueAssigneeFilter filter)
        {
            if (filter == null) return null;
            return filter.NoAssignedUser ? "No Assigned User" : filter.AssignedUser;
        }

    }
}

