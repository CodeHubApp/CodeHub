using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public class TeamMembersViewModel : BaseUserCollectionViewModel
    {
        public long Id { get; set; }

        public TeamMembersViewModel(IApplicationService applicationService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => Load(applicationService.Client.Teams[Id].GetMembers(), t as bool?));
        }
    }
}