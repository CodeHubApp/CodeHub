using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Akavache;
using Octokit.Internal;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace CodeHub.Core.Data
{
    public interface ITrendingRepository
    {
        Task<IList<Octokit.Repository>> GetTrendingRepositories(string since, string language = null);
    }

    public class TrendingRepository : ITrendingRepository
    {
        private readonly IJsonSerializer _serializer;
        private const string TrendingUrl = "http://trending.codehub-app.com/v2/trending";

        public TrendingRepository(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task<IList<Octokit.Repository>> GetTrendingRepositories(string since, string language = null)
        {
            var query = "?since=" + since;
            if (!string.IsNullOrEmpty(language))
                query += string.Format("&language={0}", language);

            var data = await BlobCache.LocalMachine.DownloadUrl(TrendingUrl + query, absoluteExpiration: DateTimeOffset.Now.AddHours(1)).ToTask().ConfigureAwait(false);
            return _serializer.Deserialize<List<Octokit.Repository>>(Encoding.UTF8.GetString(data, 0, data.Length));
        }
    }
}

