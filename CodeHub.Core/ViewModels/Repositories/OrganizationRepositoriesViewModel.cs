using System;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class OrganizationRepositoriesViewModel : BaseRepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        private string _name;
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        public OrganizationRepositoriesViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
            _applicationService = applicationService;
            this.WhenAnyValue(x => x.Name).Subscribe(x => Title = x ?? "Repositories");
            ShowRepositoryOwner = false;
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.RepositoryModel>> CreateRequest()
        {
            return _applicationService.Client.Organizations[Name].Repositories.GetAll();
        }
    }
}

