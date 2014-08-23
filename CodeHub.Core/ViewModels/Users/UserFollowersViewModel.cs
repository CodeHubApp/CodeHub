using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowersViewModel : BaseUserCollectionViewModel
    {
        public string Username { get; set; }

        public UserFollowersViewModel(IApplicationService applicationService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => Load(applicationService.Client.Users[Username].GetFollowers(), t as bool?));
        }
    }
}

