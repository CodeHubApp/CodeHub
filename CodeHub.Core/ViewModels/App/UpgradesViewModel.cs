using CodeHub.Core.ViewModels;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using System.Linq;
using System;

namespace CodeHub.Core.ViewModels.App
{
    public class UpgradesViewModel : LoadableViewModel
    {
        private readonly IFeaturesService _featuresService;
        private string[] _keys;

        public string[] Keys
        {
            get { return _keys; }
            private set 
            {
                _keys = value;
                RaisePropertyChanged(() => Keys);
            }
        }

        public UpgradesViewModel(IFeaturesService featuresService)
        {
            _featuresService = featuresService;
        }

        protected override async Task Load(bool forceCacheInvalidation)
        {
            Keys = (await _featuresService.GetAvailableFeatureIds()).ToArray();
        }
    }
}

