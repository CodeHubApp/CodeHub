using System;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public class JsonHttpClientService : IJsonHttpClientService
    {
        private readonly IHttpClientService _httpClientService;

        public JsonHttpClientService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public async Task<TMessage> Get<TMessage>(string url)
        {
            var client = _httpClientService.Create();
            client.Timeout = new TimeSpan(0, 0, 30);
            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TMessage>(data);
        }

    }
}

