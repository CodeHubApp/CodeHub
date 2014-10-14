using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowingsViewModel : BaseUserCollectionViewModel
    {
        public string Username { get; set; }

        public UserFollowingsViewModel(IApplicationService applicationService)
        {
            Title = "Following";
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => Load(applicationService.Client.Users[Username].GetFollowing(), t as bool?));
        }
    }
}