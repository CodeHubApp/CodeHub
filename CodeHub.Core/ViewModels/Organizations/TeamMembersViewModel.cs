using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamMembersViewModel : BaseUsersViewModel
    {
        public int Id { get; private set; }

        public TeamMembersViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            Title = "Members";
        }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.Teams(Id); }
        }

        public TeamMembersViewModel Init(int id)
        {
            Id = id;
            return this;
        }
    }
}