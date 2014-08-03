using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowingsViewModel : BaseUserCollectionViewModel, ILoadableViewModel
    {
        public string Username { get; set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public UserFollowingsViewModel(IApplicationService applicationService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                UsersCollection.SimpleCollectionLoad(applicationService.Client.Users[Username].GetFollowing(), t as bool?));
        }
    }
}