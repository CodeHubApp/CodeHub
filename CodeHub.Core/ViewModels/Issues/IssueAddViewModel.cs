using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueAddViewModel : IssueModifyViewModel
	{
	    private readonly ISessionService _applicationService;
        private readonly IAlertDialogFactory _alertDialogFactory;

        public IssueAddViewModel(
            ISessionService applicationService, 
            IAlertDialogFactory alertDialogService)
            : base(applicationService, alertDialogService)
        {
            _applicationService = applicationService;
            _alertDialogFactory = alertDialogService;
            Title = "New Issue";
        }

        protected override Task<Octokit.Issue> Save()
		{
			try
			{
                var labels = AssignedLabels.With(x => x.Select(y => y.Name).ToArray());
                var milestone = AssignedMilestone.With(x => (int?)x.Number);
                var user = AssignedUser.With(x => x.Login);
                var newIssue = new Octokit.NewIssue(Subject) {
                    Body = Content,
                    Assignee = user,
                    Milestone = milestone
                };

                foreach (var label in labels)
                    newIssue.Labels.Add(label);

                return _applicationService.GitHubClient.Issue.Create(RepositoryOwner, RepositoryName, newIssue);
			}
			catch (Exception e)
			{
                throw new Exception("Unable to save new issue! Please try again.", e);
			}
		}

        protected override async Task<bool> Discard()
        {
            if (string.IsNullOrEmpty(Subject) && string.IsNullOrEmpty(Content)) return true;
            return await _alertDialogFactory.PromptYesNo("Discard Issue?", "Are you sure you want to discard this issue?");
        }
    }
}

