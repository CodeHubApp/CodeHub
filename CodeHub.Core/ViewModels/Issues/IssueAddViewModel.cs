using MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using System.Linq;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAddViewModel : IssueModifyViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IApplicationService _applicationService;

        public IssueAddViewModel(IMessageService messageService, IApplicationService applicationService)
            : base(messageService)
        {
            _messageService = messageService;
            _applicationService = applicationService;
        }

        protected override async Task Save()
        {
            if (string.IsNullOrEmpty(IssueTitle))
            {
                DisplayAlert("Unable to save the issue: you must provide a title!");
                return;
            }

            try
            {
                var newIssue = new Octokit.NewIssue(IssueTitle);
                newIssue.Body = Content ?? string.Empty;
                newIssue.Milestone = Milestone?.Number;

                if (AssignedTo != null)
                    newIssue.Assignees.Add(AssignedTo.Login);

                foreach (var label in Labels.Items.Select(x => x.Name))
                    newIssue.Labels.Add(label);

                IsSaving = true;
                var data = await _applicationService.GitHubClient.Issue.Create(Username, Repository, newIssue);
                _messageService.Send(new IssueAddMessage(data));
                ChangePresentation(new MvxClosePresentationHint(this));
            }
            catch
            {
                DisplayAlert("Unable to save new issue! Please try again.");
            }
            finally
            {
                IsSaving = false;
            }
        }

        public void Init(NavObject navObject)
        {
            base.Init(navObject.Username, navObject.Repository);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

