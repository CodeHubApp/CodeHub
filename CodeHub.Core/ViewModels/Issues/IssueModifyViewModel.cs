using System;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Factories;
using System.Reactive.Linq;
using System.Collections.Generic;
using CodeHub.Core.Services;
using Octokit;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class IssueModifyViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly Lazy<Task<IReadOnlyList<User>>> _assigneesCache;
        private readonly Lazy<Task<IReadOnlyList<Milestone>>> _milestonesCache;
        private readonly Lazy<Task<IReadOnlyList<Label>>> _labelsCache;

        private string _subject;
        public string Subject
        {
            get { return _subject; }
            set { this.RaiseAndSetIfChanged(ref _subject, value); }
        }

		private string _content;
		public string Content
		{
			get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
		}

        private bool? _isCollaborator;
        public bool? IsCollaborator
        {
            get { return _isCollaborator; }
            private set { this.RaiseAndSetIfChanged(ref _isCollaborator, value); }
        }

        private IReadOnlyList<Label> _labels;
        public IReadOnlyList<Label> Labels
        {
            get { return _labels; }
            protected set { this.RaiseAndSetIfChanged(ref _labels, value); }
        }

        private Milestone _milestone;
        public Milestone Milestone
        {
            get { return _milestone; }
            protected set { this.RaiseAndSetIfChanged(ref _milestone, value); }
        }

        private User _assignee;
        public User Assignee
        {
            get { return _assignee; }
            protected set { this.RaiseAndSetIfChanged(ref _assignee, value); }
        }

        public IReactiveCommand<object> GoToAssigneesCommand { get; }

        public IReactiveCommand<object> GoToMilestonesCommand { get; }

        public IReactiveCommand<object> GoToLabelsCommand { get; }

        public IReactiveCommand<Unit> LoadCommand { get; }

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public IReactiveCommand<Issue> SaveCommand { get; }

        public IReactiveCommand<bool> DismissCommand { get; }

        protected IssueModifyViewModel(
            ISessionService sessionService, 
            IAlertDialogFactory alertDialogFactory)
	    {
            GoToAssigneesCommand = ReactiveCommand.Create();
            GoToLabelsCommand = ReactiveCommand.Create();
            GoToMilestonesCommand = ReactiveCommand.Create();

            _assigneesCache = new Lazy<Task<IReadOnlyList<User>>>(() => 
                sessionService.GitHubClient.Issue.Assignee.GetAllForRepository(RepositoryOwner, RepositoryName));
            _milestonesCache = new Lazy<Task<IReadOnlyList<Milestone>>>(() => 
                sessionService.GitHubClient.Issue.Milestone.GetAllForRepository(RepositoryOwner, RepositoryName));
            _labelsCache = new Lazy<Task<IReadOnlyList<Label>>>(() => 
                sessionService.GitHubClient.Issue.Labels.GetAllForRepository(RepositoryOwner, RepositoryName));

            var canSave = this.WhenAnyValue(x => x.Subject).Select(x => !string.IsNullOrEmpty(x));
            SaveCommand = ReactiveCommand.CreateAsyncTask(canSave, _ => {
                using (alertDialogFactory.Activate("Saving..."))
                    return Save();
            });

            // This is because the stupid ReactiveUI issue with the tableview :(
            SaveCommand.Subscribe(_ => Dismiss());

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                try
                {
                    IsCollaborator = await sessionService.GitHubClient.Repository.RepoCollaborators
                        .IsCollaborator(RepositoryOwner, RepositoryName, sessionService.Account.Username);
                }
                catch
                {
                    IsCollaborator = false;
                }
            });

            DismissCommand = ReactiveCommand.CreateAsyncTask(t => Discard());
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
	    }

        protected abstract Task<Issue> Save();

        protected abstract Task<bool> Discard();

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
                x =>  Task.FromResult(Labels = new ReadOnlyCollection<Label>(x.ToList())));
            vm.LoadCommand.ExecuteIfCan();
            return vm;
        }

    }
}

