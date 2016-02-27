using MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using GitHubSharp.Models;
using System;
using CodeHub.Core.Messages;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueEditViewModel : IssueModifyViewModel
    {
        private IssueModel _issue;
        private bool _open;

        public bool IsOpen
        {
            get { return _open; }
            set { this.RaiseAndSetIfChanged(ref _open, value); }
        }

        public IssueModel Issue
        {
            get { return _issue; }
            set { this.RaiseAndSetIfChanged(ref _issue, value); }
        }

        public long Id { get; private set; }

        protected override async Task Save()
        {
            try
            {
                if (string.IsNullOrEmpty(IssueTitle))
                    throw new Exception("Issue must have a title!");

                string assignedTo = AssignedTo == null ? null : AssignedTo.Login;
                int? milestone = null;
                if (Milestone != null) 
                    milestone = Milestone.Number;
                string[] labels = Labels.Items.Select(x => x.Name).ToArray();
                var content = Content ?? string.Empty;
                var state = IsOpen ? "open" : "closed";
                var retried = false;

                IsSaving = true;

                // For some reason github needs to try again during an internal server error
                tryagain:

                try
                {
                    var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Issue.Number].Update(IssueTitle, content, state, assignedTo, milestone, labels)); 
                    Messenger.Publish(new IssueEditMessage(this) { Issue = data.Data });
                }
                catch (GitHubSharp.InternalServerException)
                {
                    if (retried)
                        throw;

                    //Do nothing. Something is wrong with github's service
                    retried = true;
                    goto tryagain;
                }

                ChangePresentation(new MvxClosePresentationHint(this));
            }
            catch
            {
                DisplayAlert("Unable to save the issue! Please try again");
            }
            finally
            {
                IsSaving = false;
            }

//            //There is a wierd bug in GitHub when editing an existing issue and the assignedTo is null
//            catch (GitHubSharp.InternalServerException)
//            {
//                if (ExistingIssue != null && assignedTo == null)
//                    tryEditAgain = true;
//                else
//                    throw;
//            }
//
//            if (tryEditAgain)
//            {
//                var response = await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepoSlug].Issues[ExistingIssue.Number].Update(title, content, state, assignedTo, milestone, labels)); 
//                model = response.Data;
//            }
        }

//        protected override Task Load(bool forceCacheInvalidation)
//        {
//            if (forceCacheInvalidation || Issue == null)
//                return Task.Run(() => this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), forceCacheInvalidation, response => Issue = response.Data));
//            return Task.Delay(0);
//        }

        public void Init(NavObject navObject)
        {
            base.Init(navObject.Username, navObject.Repository);
            Id = navObject.Id;
            Issue = GetService<CodeHub.Core.Services.IViewModelTxService>().Get() as IssueModel;
            if (Issue != null)
            {
                IssueTitle = Issue.Title;
                AssignedTo = Issue.Assignee;
                Milestone = Issue.Milestone;
                Labels.Items.Reset(Issue.Labels);
                Content = Issue.Body;
                IsOpen = string.Equals(Issue.State, "open");
            }
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public long Id { get; set; }
        }
    }
}

