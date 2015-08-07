using System;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class OrganizationRepositoriesViewModel : BaseRepositoriesViewModel
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        public OrganizationRepositoriesViewModel(ISessionService applicationService)
            : base(applicationService)
        {
            this.WhenAnyValue(x => x.Name).Subscribe(x => Title = x ?? "Repositories");
            ShowRepositoryOwner = false;
        }

        protected override Uri RepositoryUri
        {
            get { return Octokit.ApiUrls.OrganizationRepositories(Name); }
        }
    }
}

