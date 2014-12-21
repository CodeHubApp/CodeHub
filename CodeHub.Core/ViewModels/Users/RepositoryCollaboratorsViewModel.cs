using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class RepositoryCollaboratorsViewModel : BaseUserCollectionViewModel
    {
        private readonly IApplicationService _applicationService;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public RepositoryCollaboratorsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Title = "Collaborators";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetCollaborators();
        }
    }
}

