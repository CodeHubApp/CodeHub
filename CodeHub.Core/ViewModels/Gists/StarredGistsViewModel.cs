using CodeHub.Core.Services;
using Xamarin.Utilities.Core.ViewModels;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class StarredGistsViewModel : BaseGistsViewModel, ILoadableViewModel
    {
        public IReactiveCommand LoadCommand { get; private set; }

        public StarredGistsViewModel(IApplicationService applicationService)
        {
            Title = "Starred Gists";
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                GistsCollection.SimpleCollectionLoad(applicationService.Client.Gists.GetStarredGists(), t as bool?));
            LoadCommand.ExecuteIfCan();
        }
    }
}
