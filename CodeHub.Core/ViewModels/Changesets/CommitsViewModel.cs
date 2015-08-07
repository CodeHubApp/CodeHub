using CodeHub.Core.Services;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.Changesets
{
	public class CommitsViewModel : BaseCommitsViewModel
	{
	    public string Branch { get; private set; }

        public CommitsViewModel(ISessionService sessionService)
            : base(sessionService)
        {
        }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.RepositoryCommits(RepositoryOwner, RepositoryName); }
        }

        protected override void AddRequestParameters(IDictionary<string, string> parameters)
        {
            parameters["sha"] = Branch ?? "master";
        }

        public CommitsViewModel Init(string repositoryOwner, string repositoryName, string branch)
        {
            Init(repositoryOwner, repositoryName);
            Branch = branch;
            return this;
        }
    }
}

