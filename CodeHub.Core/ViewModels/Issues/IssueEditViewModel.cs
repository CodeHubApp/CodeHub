using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using System;
using ReactiveUI;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueEditViewModel : IssueModifyViewModel
    {
        private readonly ISessionService _sessionService;
        private readonly IAlertDialogFactory _alertDialogFactory;
        private Octokit.Issue _issue;
		private bool _open;

		public bool IsOpen
		{
			get { return _open; }
			set { this.RaiseAndSetIfChanged(ref _open, value); }
		}

        public Octokit.Issue Issue
		{
			get { return _issue; }
			set { this.RaiseAndSetIfChanged(ref _issue, value); }
		}

		public int Id { get; set; }

        public IssueEditViewModel(
            ISessionService sessionService, 
            IAlertDialogFactory alertDialogFactory)
            : base(sessionService, alertDialogFactory)
	    {
            _sessionService = sessionService;
            _alertDialogFactory = alertDialogFactory;

            Title = "Edit Issue";

	        this.WhenAnyValue(x => x.Issue)
                .IsNotNull()
                .Subscribe(x => {
                    Title = string.Format("Edit Issue #{0}", x.Number);
                    Subject = x.Title;
                    Assignees.Selected = x.Assignee;
                    Milestones.Selected = x.Milestone;
                    Labels.Selected = x.Labels;
                    Content = x.Body;
                    IsOpen = x.State == Octokit.ItemState.Open;
    	        });
	    }

        protected override async Task<bool> Discard()
        {
            if (string.IsNullOrEmpty(Subject) && string.IsNullOrEmpty(Content)) return true;
            return await _alertDialogFactory.PromptYesNo("Discard Issue?", "Are you sure you want to discard this update?");
        }

        protected override Task<Octokit.Issue> Save()
		{
            try
            {
                var labels = Labels.Selected?.Select(y => y.Name).ToArray();
                var milestone = Milestones.Selected?.Number;
                var user = Assignees.Selected?.Login;
                var issueUpdate = new Octokit.IssueUpdate {
                    Body = Content,
                    Assignee = user,
                    Milestone = milestone,
                    Title = Subject,
                };

                if (labels != null && labels.Length > 0)
                {
                    foreach (var label in labels)
                        issueUpdate.AddLabel(label);
                }

                return _sessionService.GitHubClient.Issue.Update(RepositoryOwner, RepositoryName, Id, issueUpdate);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to update issue! Please try again.", e);
            }
		}

//		protected override Task Load(bool forceCacheInvalidation)
//		{
//			if (forceCacheInvalidation || Issue == null)
//				return Task.Run(() => this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), forceCacheInvalidation, response => Issue = response.Data));
//			return Task.Delay(0);
//		}
    }
}

