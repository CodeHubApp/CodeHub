using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public class OrganizationMembersViewModel : BaseUserCollectionViewModel, ILoadableViewModel
    {
        public string OrganizationName { get; set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public OrganizationMembersViewModel(IApplicationService applicationService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(
                t => Users.SimpleCollectionLoad(applicationService.Client.Organizations[OrganizationName].GetMembers(), t as bool?));
        }
    }
}

