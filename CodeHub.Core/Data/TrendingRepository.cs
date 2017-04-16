using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace CodeHub.Core.Data
{
    public interface ITrendingRepository
    {
        Task<IList<Octokit.Repository>> GetTrendingRepositories(string since, string language = null);
    }

    public class TrendingRepository : ITrendingRepository
    {
        private const string TrendingUrl = "http://trending.codehub-app.com/v2/trending";

        public async Task<IList<Octokit.Repository>> GetTrendingRepositories(string since, string language = null)
        {
            var query = "?since=" + Uri.EscapeDataString(since);
            if (!string.IsNullOrEmpty(language))
                query += string.Format("&language={0}", Uri.EscapeDataString(language));

            var client = new HttpClient();
            var serializer = new Octokit.Internal.SimpleJsonSerializer();
            var msg = await client.GetAsync(TrendingUrl + query).ConfigureAwait(false);
            var content = await msg.Content.ReadAsStringAsync().ConfigureAwait(false);
            return serializer.Deserialize<List<Octokit.Repository>>(content);
        }
    }
}

