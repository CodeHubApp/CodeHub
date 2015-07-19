using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamMembersViewModel : BaseUsersViewModel
    {
        private readonly ISessionService _sessionService;

        public long Id { get; set; }

        public TeamMembersViewModel(ISessionService applicationService)
        {
            _sessionService = applicationService;
            Title = "Members";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _sessionService.Client.Teams[Id].GetMembers();
        }
    }
}