using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Changesets
{
	public class CommitsViewModel : BaseCommitsViewModel
	{
        private readonly ISessionService _applicationService;

	    public string Branch { get; private set; }

        public CommitsViewModel(ISessionService applicationService)
        {
            _applicationService = applicationService;
        }

		protected override GitHubRequest<List<CommitModel>> CreateRequest()
        {
            return _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits.GetAll(Branch ?? "master");
        }

        public CommitsViewModel Init(string repositoryOwner, string repositoryName, string branch)
        {
            Init(repositoryOwner, repositoryName);
            Branch = branch;
            return this;
        }
    }
}

