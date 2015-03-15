using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class OrganizationMembersViewModel : BaseUsersViewModel
    {
        private readonly ISessionService _sessionService;

        public string OrganizationName { get; private set; }

        public OrganizationMembersViewModel(ISessionService applicationService)
        {
            _sessionService = applicationService;
            Title = "Members";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _sessionService.Client.Organizations[OrganizationName].GetMembers();
        }

        public OrganizationMembersViewModel Init(string organizationName)
        {
            OrganizationName = organizationName;
            return this;
        }
    }
}

