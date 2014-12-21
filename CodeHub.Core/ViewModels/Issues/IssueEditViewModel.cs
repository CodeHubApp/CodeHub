using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System;
using ReactiveUI;
using Xamarin.Utilities.Services;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueEditViewModel : IssueModifyViewModel
    {
	    private readonly IApplicationService _applicationService;
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

		public long Id { get; set; }

        public IReactiveCommand<object> GoToDescriptionCommand { get; private set; }

        public IssueEditViewModel(IApplicationService applicationService, IStatusIndicatorService statusIndicatorService)
            : base(statusIndicatorService)
	    {
	        _applicationService = applicationService;

            Title = "Edit Issue";

            GoToDescriptionCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null));
//	        GoToDescriptionCommand.Subscribe(_ =>
//	        {
//	            var vm = this.CreateViewModel<ComposerViewModel>();
//	            vm.Text = Issue.Body;
//	            vm.SaveCommand.Subscribe(__ =>
//	            {
//	                Issue.Body = vm.Text;
//                    vm.DismissCommand.ExecuteIfCan();
//	            });
//	            NavigateTo(vm);
//	        });

	        this.WhenAnyValue(x => x.Issue).Where(x => x != null).Subscribe(x =>
	        {
//                Title = x.Title;
//                AssignedTo = x.Assignee;
//                Milestone = x.Milestone;
//                Labels = x.Labels.ToArray();
//                Content = x.Body;
//                IsOpen = string.Equals(x.State, "open");
	        });
	    }

	    protected override async Task Save()
		{
            if (string.IsNullOrEmpty(Title))
                throw new Exception("Issue must have a title!");

			try
			{
				var assignedTo = AssignedTo == null ? null : AssignedTo.Login;
				int? milestone = null;
				if (Milestone != null) 
					milestone = Milestone.Number;
				var labels = Labels.Select(x => x.Name).ToArray();
				var content = Content ?? string.Empty;
				var state = IsOpen ? "open" : "closed";
				var retried = false;

				// For some reason github needs to try again during an internal server error
				tryagain:

				try
				{
                    var data = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Issue.Number].Update(Title, content, state, assignedTo, milestone, labels));
				    Issue = data.Data;
				}
				catch (GitHubSharp.InternalServerException)
				{
					if (retried)
						throw;

					//Do nothing. Something is wrong with github's service
					retried = true;
					goto tryagain;
				}
			}
			catch (Exception e)
			{
                throw new Exception("Unable to save the issue! Please try again", e);
			}

//			//There is a wierd bug in GitHub when editing an existing issue and the assignedTo is null
//			catch (GitHubSharp.InternalServerException)
//			{
//				if (ExistingIssue != null && assignedTo == null)
//					tryEditAgain = true;
//				else
//					throw;
//			}
//
//			if (tryEditAgain)
//			{
//				var response = await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepoSlug].Issues[ExistingIssue.Number].Update(title, content, state, assignedTo, milestone, labels)); 
//				model = response.Data;
//			}
		}

//		protected override Task Load(bool forceCacheInvalidation)
//		{
//			if (forceCacheInvalidation || Issue == null)
//				return Task.Run(() => this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), forceCacheInvalidation, response => Issue = response.Data));
//			return Task.Delay(0);
//		}
    }
}

