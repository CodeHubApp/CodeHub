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
    public class IssueAssignedToViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;

        private Octokit.User _selectedUser;
        public Octokit.User SelectedUser
        {
            get => _selectedUser;
            set => this.RaiseAndSetIfChanged(ref _selectedUser, value);
        }

        public ReactiveList<Octokit.User> Users { get; } = new ReactiveList<Octokit.User>();

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public int Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public ReactiveCommand<Octokit.User, Unit> SelectUserCommand { get; }

        public IssueAssignedToViewModel(
            string username,
            string repository,
            int id,
            Octokit.User selectedUser = null,
            bool saveOnSelect = false,
            IMessageService messageService = null)
        {
            Username = username;
            Repository = repository;
            Id = id;
            SaveOnSelect = saveOnSelect;
            SelectedUser = selectedUser;
            _messageService = messageService ?? Locator.Current.GetService<IMessageService>();

            SelectUserCommand = ReactiveCommand.CreateFromTask<Octokit.User>(SelectUser);

            SelectUserCommand
                .ThrownExceptions
                .Select(err => new UserError("Unable to assign issue to selected user! Please try again."))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            this.WhenAnyValue(x => x.SelectedUser)
                .Skip(1)
                .InvokeReactiveCommand(SelectUserCommand);
        }

        private async Task SelectUser(Octokit.User x)
        {
            if (SaveOnSelect)
            {
                var update = new Octokit.IssueUpdate();
                update.AddAssignee(x?.Login);
                var result = await this.GetApplication().GitHubClient.Issue.Update(Username, Repository, Id, update);
                _messageService.Send(new IssueEditMessage(result));
            }
            else
            {
                _messageService.Send(new SelectedAssignedToMessage(x));
            }
        }

        protected override async Task Load()
        {
            var result = await this.GetApplication().GitHubClient.Issue.Assignee.GetAllForRepository(Username, Repository);
            Users.Reset(result);
        }
    }
}

