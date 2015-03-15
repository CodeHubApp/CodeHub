using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Akavache;
using System.Text;
using System.Collections.Generic;

namespace CodeHub.Core.Data
{
    public class ShowcaseRepository
    {
        private const string ShowcaseUrl = "http://trending.codehub-app.com/v2/showcases/{0}";

        public async Task<ShowcaseRepositories> GetShowcaseRepositories(string showcaseSlug)
        {
            var url = string.Format(ShowcaseUrl, showcaseSlug);
            var data = await BlobCache.LocalMachine.DownloadUrl(url, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
            var serializer = new Octokit.Internal.SimpleJsonSerializer();
            var decodedData = Encoding.UTF8.GetString(data, 0, data.Length);
            return serializer.Deserialize<ShowcaseRepositories>(decodedData);
        }

        public async Task<List<Showcase>> GetShowcases()
        {
            var url = string.Format(ShowcaseUrl, string.Empty);
            var data = await BlobCache.LocalMachine.DownloadUrl(url, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
            var serializer = new Octokit.Internal.SimpleJsonSerializer();
            var decodedData = Encoding.UTF8.GetString(data, 0, data.Length);
            return serializer.Deserialize<List<Showcase>>(decodedData);
        }
    }
}

