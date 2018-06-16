using System;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using Splat;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class IssueModifyViewModel : BaseViewModel
    {
        private IDisposable _labelsToken, _milestoneToken, _assignedToken;

        private string _title;
        public string IssueTitle
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }

        private Octokit.Milestone _milestone;
        public Octokit.Milestone Milestone
        {
            get { return _milestone; }
            set { this.RaiseAndSetIfChanged(ref _milestone, value); }
        }

        public ReactiveList<Octokit.Label> Labels { get; } = new ReactiveList<Octokit.Label>();

        private Octokit.User _assignedTo;
        public Octokit.User AssignedTo
        {
            get { return _assignedTo; }
            set { this.RaiseAndSetIfChanged(ref _assignedTo, value); }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            protected set { this.RaiseAndSetIfChanged(ref _isSaving, value); }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        protected IMessageService MessageService { get; }

        protected IssueModifyViewModel(
            string username,
            string repository,
            IMessageService messageService = null)
        {
            Username = username;
            Repository = repository;
            MessageService = messageService ?? Locator.Current.GetService<IMessageService>();

            _labelsToken = MessageService.Listen<SelectIssueLabelsMessage>(x => Labels.Reset(x.Labels));
            _milestoneToken = MessageService.Listen<SelectedMilestoneMessage>(x => Milestone = x.Milestone);
            _assignedToken = MessageService.Listen<SelectedAssignedToMessage>(x => AssignedTo = x.User);

            SaveCommand = ReactiveCommand.CreateFromTask(Save);

            SaveCommand
                .ThrownExceptions
                .Select(err => new UserError(err.Message))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();
        }

        protected abstract Task Save();
    }
}

