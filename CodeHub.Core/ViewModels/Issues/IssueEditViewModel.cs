using System;
using System.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueEditViewModel : IssueModifyViewModel
    {
        public int Id { get; }

        private bool _open;
        public bool IsOpen
        {
            get => _open;
            set => this.RaiseAndSetIfChanged(ref _open, value);
        }

        private Octokit.Issue _issue;
        public Octokit.Issue Issue
        {
            get => _issue;
            set => this.RaiseAndSetIfChanged(ref _issue, value);
        }

        public IssueEditViewModel(
            string username,
            string repository,
            Octokit.Issue issue,
            IMessageService messageService = null)
            : base(username, repository, messageService)
        {
            Issue = issue;
            Id = issue.Number;

            IssueTitle = Issue.Title;
            AssignedTo = Issue.Assignee;
            Milestone = Issue.Milestone;
            Labels.Reset(Issue.Labels);
            Content = Issue.Body;
            IsOpen = Issue.State.Value == Octokit.ItemState.Open;
        }

        protected override async Task Save()
        {
            if (string.IsNullOrEmpty(IssueTitle))
                throw new Exception("Issue must have a title!");

            var issue = new Octokit.IssueUpdate
            {
                Body = Content ?? string.Empty,
                Milestone = Milestone?.Number,
                State = IsOpen ? Octokit.ItemState.Open : Octokit.ItemState.Closed
            };

            if (AssignedTo != null)
                issue.Assignees.Add(AssignedTo.Login);

            foreach (var label in Labels.Select(x => x.Name))
                issue.Labels.Add(label);

            var result = await this.GetApplication().GitHubClient.Issue.Update(Username, Repository, Issue.Number, issue);
            MessageService.Send(new IssueEditMessage(result));
        }
    }
}

