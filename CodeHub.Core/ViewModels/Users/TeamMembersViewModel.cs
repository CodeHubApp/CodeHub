using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class TeamMembersViewModel : BaseUserCollectionViewModel
    {
        public long Id { get; set; }

        public TeamMembersViewModel(IApplicationService applicationService)
        {
            LoadCommand.RegisterAsyncTask(
                t => Users.SimpleCollectionLoad(applicationService.Client.Teams[Id].GetMembers(), t as bool?));
        }
    }
}