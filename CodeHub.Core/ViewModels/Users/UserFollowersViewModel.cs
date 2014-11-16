using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowersViewModel : BaseUserCollectionViewModel
    {
        public string Username { get; set; }

        public UserFollowersViewModel(IApplicationService applicationService)
        {
            Title = "Followers";
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => Load(applicationService.Client.Users[Username].GetFollowers(), t as bool?));
        }
    }
}

