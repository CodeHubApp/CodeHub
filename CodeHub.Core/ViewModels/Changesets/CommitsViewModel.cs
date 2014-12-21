using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Changesets
{
	public class CommitsViewModel : BaseCommitsViewModel
	{
        private readonly IApplicationService _applicationService;

	    public string Branch { get; set; }

        public CommitsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

		protected override GitHubRequest<List<CommitModel>> CreateRequest()
        {
            return _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits.GetAll(Branch ?? "master");
        }
    }
}

