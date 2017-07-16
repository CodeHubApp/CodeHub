using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public class ImgurService : IImgurService
    {
        private const string AuthorizationClientId = "4d2779fd2cc56cb";
        private const string ImgurPostUrl = "https://api.imgur.com/3/image";

        public async Task<ImgurResponse> SendImage(byte[] data)
        {
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 30);
            client.DefaultRequestHeaders.Add("Authorization", "Client-ID " + AuthorizationClientId);
            var body = JsonConvert.SerializeObject(new { image = Convert.ToBase64String(data) });
            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(ImgurPostUrl, content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException("Unable to post to Imgur! " + response.ReasonPhrase);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ImgurResponse>(responseBody);
        }
    }
}

