using System;
using Cirrious.MvvmCross.ViewModels;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueAddViewModel : IssueModifyViewModel
    {
		protected override async Task Save()
		{
			try
			{
				if (string.IsNullOrEmpty(Title))
					throw new Exception("Issue must have a title!");

				string assignedTo = AssignedTo == null ? null : AssignedTo.Login;
				int? milestone = null;
				if (Milestone != null) 
					milestone = Milestone.Number;
				string[] labels = Labels.Items.Select(x => x.Name).ToArray();
				var content = Content ?? string.Empty;

				IsSaving = true;
				var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues.Create(Title, content, assignedTo, milestone, labels));
				Messenger.Publish(new IssueAddMessage(this) { Issue = data.Data });
				ChangePresentation(new MvxClosePresentationHint(this));
			}
			catch (Exception e)
			{
				ReportError(e);
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

