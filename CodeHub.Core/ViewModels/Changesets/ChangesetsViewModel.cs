using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Changesets
{
	public class ChangesetsViewModel : CommitsViewModel
	{
	    protected readonly IApplicationService ApplicationService;

	    public string Branch { get; set; }

        public ChangesetsViewModel(IApplicationService applicationService)
        {
            ApplicationService = applicationService;
        }

		protected override GitHubRequest<List<CommitModel>> GetRequest()
        {
            return ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits.GetAll(Branch ?? "master");
        }
    }
}

