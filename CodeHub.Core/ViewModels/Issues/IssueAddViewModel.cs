using System;
using MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAddViewModel : IssueModifyViewModel
    {
        protected override async Task Save()
        {
            if (string.IsNullOrEmpty(IssueTitle))
            {
                DisplayAlert("Unable to save the issue: you must provide a title!");
                return;
            }

            try
            {
                string assignedTo = AssignedTo == null ? null : AssignedTo.Login;
                int? milestone = null;
                if (Milestone != null) 
                    milestone = Milestone.Number;
                string[] labels = Labels.Items.Select(x => x.Name).ToArray();
                var content = Content ?? string.Empty;

                IsSaving = true;
                var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues.Create(IssueTitle, content, assignedTo, milestone, labels));
                Messenger.Publish(new IssueAddMessage(this) { Issue = data.Data });
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

