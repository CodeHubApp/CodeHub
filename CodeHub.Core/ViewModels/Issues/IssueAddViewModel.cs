using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Reactive.Subjects;
using CodeHub.Core.Factories;
using System.Collections.Generic;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueAddViewModel : IssueModifyViewModel
	{
        private readonly Subject<IssueModel> _createdIssueSubject = new Subject<IssueModel>();
	    private readonly IApplicationService _applicationService;

        public IObservable<IssueModel> CreatedIssue
        {
            get { return _createdIssueSubject; }
        }

        public IssueAddViewModel(
            IApplicationService applicationService, 
            IAlertDialogFactory statusIndicatorService)
            : base(applicationService, statusIndicatorService)
        {
            _applicationService = applicationService;
            Title = "New Issue";
        }

		protected override async Task Save()
		{
			try
			{
                var request = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues.Create(Subject, Content, null, null, new string[0]);
                var data = await _applicationService.Client.ExecuteAsync(request);
                _createdIssueSubject.OnNext(data.Data);
			}
			catch (Exception e)
			{
                throw new Exception("Unable to save new issue! Please try again.", e);
			}
		}
    }
}

