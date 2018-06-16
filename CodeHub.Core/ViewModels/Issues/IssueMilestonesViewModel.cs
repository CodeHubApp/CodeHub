using System.Threading.Tasks;
using CodeHub.Core.Messages;
using System;
using CodeHub.Core.Services;
using ReactiveUI;
using Splat;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;

        private Octokit.Milestone _selectedMilestone;
        public Octokit.Milestone SelectedMilestone
        {
            get => _selectedMilestone;
            set => this.RaiseAndSetIfChanged(ref _selectedMilestone, value);
        }

        public ReactiveList<Octokit.Milestone> Milestones { get; } = new ReactiveList<Octokit.Milestone>();

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public int Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        private ReactiveCommand<Octokit.Milestone, Unit> SelectMilstoneCommand { get; }

        public IssueMilestonesViewModel(
            string username,
            string repository,
            int id,
            Octokit.Milestone selectedMilestone = null,
            bool saveOnSelect = false,
            IMessageService messageService = null)
        {
            Username = username;
            Repository = repository;
            Id = id;
            SaveOnSelect = saveOnSelect;
            SelectedMilestone = selectedMilestone;

            _messageService = messageService ?? Locator.Current.GetService<IMessageService>();

            SelectMilstoneCommand = ReactiveCommand.CreateFromTask<Octokit.Milestone>(SelectMilestone);

            SelectMilstoneCommand
                .ThrownExceptions
                .Select(err => new UserError("Unable to assign milestone! Please try again."))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            this.WhenAnyValue(x => x.SelectedMilestone)
                .Skip(1)
                .InvokeReactiveCommand(SelectMilstoneCommand);
        }

        private async Task SelectMilestone(Octokit.Milestone x)
        {
            if (SaveOnSelect)
            {
                var update = new Octokit.IssueUpdate { Milestone = x?.Number };
                var result = await this.GetApplication().GitHubClient.Issue.Update(Username, Repository, Id, update);
                _messageService.Send(new IssueEditMessage(result));
            }
            else
            {
                _messageService.Send(new SelectedMilestoneMessage(x));
            }
        }

        protected override async Task Load()
        {
            var result = await this.GetApplication().GitHubClient.Issue.Milestone.GetAllForRepository(Username, Repository);
            Milestones.Reset(result);
        }
    }
}

