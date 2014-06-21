using System;
using System.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueAddViewModel : IssueModifyViewModel
	{
	    private readonly IApplicationService _applicationService;
	    private IssueModel _issue;

	    public IssueModel Issue
	    {
	        get { return _issue; }
	        private set { this.RaiseAndSetIfChanged(ref _issue, value); }
	    }

	    public IReactiveCommand GoToDescriptionCommand { get; private set; }

        public IssueAddViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            GoToDescriptionCommand = new ReactiveCommand();
            GoToDescriptionCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<MarkdownComposerViewModel>();
                vm.Text = Content;
                vm.SaveCommand.Subscribe(__ =>
                {
                    Content = vm.Text;
                    vm.DismissCommand.ExecuteIfCan();
                });
                ShowViewModel(vm);
            });
        }

		protected override async Task Save()
		{
            if (string.IsNullOrEmpty(Title))
                throw new Exception("Unable to save the issue: you must provide a title!");

			try
			{
				var assignedTo = AssignedTo == null ? null : AssignedTo.Login;
				int? milestone = null;
				if (Milestone != null) 
					milestone = Milestone.Number;
				var labels = Labels.Select(x => x.Name).ToArray();
				var content = Content ?? string.Empty;

                var data = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues.Create(Title, content, assignedTo, milestone, labels));
			    Issue = data.Data;
                DismissCommand.ExecuteIfCan();
			}
			catch (Exception e)
			{
                throw new Exception("Unable to save new issue! Please try again.", e);
			}
		}
    }
}

