using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class OrganizationMembersViewModel : BaseUserCollectionViewModel
    {
        private readonly IApplicationService _applicationService;

        public string OrganizationName { get; set; }

        public OrganizationMembersViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Title = "Members";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _applicationService.Client.Organizations[OrganizationName].GetMembers();
        }
    }
}

