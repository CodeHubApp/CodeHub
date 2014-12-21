using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Akavache;
using System.Reactive.Linq;
using Newtonsoft.Json.Serialization;

namespace CodeHub.Core.Data
{
    public class TrendingRepository
    {
        private const string TrendingUrl = "http://trending.codehub-app.com/v2/trending";

        public async Task<List<Octokit.Repository>> GetTrendingRepositories(string since, string language = null)
        {
            var query = "?since=" + since;
            if (!string.IsNullOrEmpty(language))
                query += string.Format("&language={0}", language);
            var data = await BlobCache.LocalMachine.DownloadUrl(TrendingUrl + query, absoluteExpiration: DateTimeOffset.Now.AddHours(1));
            return JsonConvert.DeserializeObject<List<Octokit.Repository>>(Encoding.UTF8.GetString(data, 0, data.Length), new JsonSerializerSettings {
                ContractResolver = new UnderscoreContractResolver()
            });
        }
    }
}

