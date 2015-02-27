using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class TeamMembersViewModel : BaseUsersViewModel
    {
        private readonly ISessionService _applicationService;

        public long Id { get; set; }

        public TeamMembersViewModel(ISessionService applicationService)
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