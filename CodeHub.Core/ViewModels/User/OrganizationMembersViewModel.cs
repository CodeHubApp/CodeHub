using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.User
{
    public class OrganizationMembersViewModel : BaseUserCollectionViewModel
    {
        public string OrganizationName { get; set; }

        public OrganizationMembersViewModel(IApplicationService applicationService)
        {
            LoadCommand.RegisterAsyncTask(
                t => Users.SimpleCollectionLoad(applicationService.Client.Organizations[OrganizationName].GetMembers(), t as bool?));
        }
    }
}

