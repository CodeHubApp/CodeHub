using System;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using System.Linq;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAddViewModel : IssueModifyViewModel
    {
        private readonly IMessageService _messageService;

        public IssueAddViewModel(
            string username,
            string repository,
            IMessageService messageService = null)
            : base(username, repository, messageService)
        {
        }

        protected override async Task Save()
        {
            if (string.IsNullOrEmpty(IssueTitle))
                throw new Exception("Unable to save the issue: you must provide a title!");

            var newIssue = new Octokit.NewIssue(IssueTitle)
            {
                Body = Content ?? string.Empty,
                Milestone = Milestone?.Number
            };

            if (AssignedTo != null)
                newIssue.Assignees.Add(AssignedTo.Login);

            foreach (var label in Labels.Select(x => x.Name))
                newIssue.Labels.Add(label);

            var result = await this.GetApplication().GitHubClient.Issue.Create(Username, Repository, newIssue);
            MessageService.Send(new IssueAddMessage(result));
        }
    }
}

