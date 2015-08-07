using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationMembersViewModel : BaseUsersViewModel
    {
        public string OrganizationName { get; private set; }

        public OrganizationMembersViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            Title = "Members";
        }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.Members(OrganizationName); }
        }

        public OrganizationMembersViewModel Init(string organizationName)
        {
            OrganizationName = organizationName;
            return this;
        }
    }
}

