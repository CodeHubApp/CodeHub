using System;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class OrganizationRepositoriesViewModel : BaseRepositoriesViewModel
    {
        public string Name { get; private set; }

        public OrganizationRepositoriesViewModel(ISessionService applicationService)
            : base(applicationService)
        {
            ShowRepositoryOwner = false;
        }

        public OrganizationRepositoriesViewModel Init(string name)
        {
            Name = name;
            Title = name ?? "Repositories";
            return this;
        }

        protected override Uri RepositoryUri
        {
            get { return Octokit.ApiUrls.OrganizationRepositories(Name); }
        }
    }
}

