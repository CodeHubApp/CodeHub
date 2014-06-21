using CodeHub.Core.Services;
using System.Linq;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
    public class UpgradesViewModel : LoadableViewModel
    {
        private string[] _keys;

        public string[] Keys
        {
            get { return _keys; }
            private set { this.RaiseAndSetIfChanged(ref _keys, value); }
        }

        public UpgradesViewModel(IFeaturesService featuresService)
        {
            LoadCommand.RegisterAsyncTask(async _ =>
            {
                Keys = (await featuresService.GetAvailableFeatureIds()).ToArray();
            });
        }
    }
}

