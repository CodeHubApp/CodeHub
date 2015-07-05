using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Akavache;
using System.Reactive.Linq;

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
            var serializer = new Octokit.Internal.SimpleJsonSerializer();
            var decodedData = Encoding.UTF8.GetString(data, 0, data.Length);
            return serializer.Deserialize<List<Octokit.Repository>>(decodedData);
        }
    }
}

