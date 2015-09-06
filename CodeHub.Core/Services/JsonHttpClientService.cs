using System;
using System.Threading.Tasks;

namespace CodeFramework.Core.Services
{
    public class JsonHttpClientService : IJsonHttpClientService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly IJsonSerializationService _jsonSerializationService;

        public JsonHttpClientService(IHttpClientService httpClientService, IJsonSerializationService jsonSerializationService)
        {
            _httpClientService = httpClientService;
            _jsonSerializationService = jsonSerializationService;
        }

        public async Task<TMessage> Get<TMessage>(string url)
        {
            var client = _httpClientService.Create();
            client.Timeout = new TimeSpan(0, 0, 30);
            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            return _jsonSerializationService.Deserialize<TMessage>(data);
        }

    }
}

