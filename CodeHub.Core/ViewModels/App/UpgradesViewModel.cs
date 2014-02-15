using System;
using CodeFramework.Core.ViewModels;
using System.Threading.Tasks;
using CodeFramework.Core.Services;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.App
{
    public class UpgradesViewModel : LoadableViewModel
    {
        private readonly IHttpClientService _httpClientService;
        private readonly IJsonSerializationService _jsonSerializationService;
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

        public UpgradesViewModel(IHttpClientService httpClientService, IJsonSerializationService jsonSerializationService)
        {
            _httpClientService = httpClientService;
            _jsonSerializationService = jsonSerializationService;
        }

        protected override async Task Load(bool forceCacheInvalidation)
        {
            var client = _httpClientService.Create();
            client.Timeout = new TimeSpan(0, 0, 30);
            var response = await client.GetAsync("http://162.243.15.10/in-app");
            var data = await response.Content.ReadAsStringAsync();
            Keys = _jsonSerializationService.Deserialize<List<string>>(data).ToArray();
        }
    }
}

