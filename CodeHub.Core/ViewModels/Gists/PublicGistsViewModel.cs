using CodeHub.Core.Services;
using Xamarin.Utilities.Core.ViewModels;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class PublicGistsViewModel : BaseGistsViewModel, ILoadableViewModel
    {
        public IReactiveCommand LoadCommand { get; private set; }

        public PublicGistsViewModel(IApplicationService applicationService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                GistsCollection.SimpleCollectionLoad(applicationService.Client.Gists.GetPublicGists(), t as bool?));
            LoadCommand.ExecuteIfCan();
        }
    }
}
