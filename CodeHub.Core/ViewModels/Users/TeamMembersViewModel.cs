using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public class TeamMembersViewModel : BaseUserCollectionViewModel, ILoadableViewModel
    {
        public long Id { get; set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public TeamMembersViewModel(IApplicationService applicationService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(
                t => UsersCollection.SimpleCollectionLoad(applicationService.Client.Teams[Id].GetMembers(), t as bool?));
        }
    }
}