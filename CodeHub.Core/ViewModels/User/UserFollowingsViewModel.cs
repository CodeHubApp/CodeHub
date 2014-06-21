using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.User
{
    public class UserFollowingsViewModel : BaseUserCollectionViewModel
    {
        public string Username { get; set; }

        public UserFollowingsViewModel(IApplicationService applicationService)
        {
            LoadCommand.RegisterAsyncTask(t =>
                Users.SimpleCollectionLoad(applicationService.Client.Users[Username].GetFollowing(), t as bool?));
        }
    }
}