using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class TeamMembersViewModel : BaseUserCollectionViewModel
    {
        private readonly IApplicationService _applicationService;

        public long Id { get; set; }

        public TeamMembersViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Title = "Members";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _applicationService.Client.Teams[Id].GetMembers();
        }
    }
}