using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowersViewModel : BaseUserCollectionViewModel, ILoadableViewModel
    {
        public string Username { get; set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public UserFollowersViewModel(IApplicationService applicationService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                    Users.SimpleCollectionLoad(applicationService.Client.Users[Username].GetFollowers(), t as bool?));
        }
    }
}

