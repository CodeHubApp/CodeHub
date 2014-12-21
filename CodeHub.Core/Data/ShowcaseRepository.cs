using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Akavache;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace CodeHub.Core.Data
{
    public class ShowcaseRepository
    {
        private const string ShowcaseUrl = "http://trending.codehub-app.com/v2/showcases/{0}";

        public async Task<ShowcaseRepositories> GetShowcaseRepositories(string showcaseSlug)
        {
            var url = string.Format(ShowcaseUrl, showcaseSlug);
            var data = await BlobCache.LocalMachine.DownloadUrl(url, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
            return JsonConvert.DeserializeObject<ShowcaseRepositories>(Encoding.UTF8.GetString(data, 0, data.Length), new JsonSerializerSettings {
                ContractResolver = new UnderscoreContractResolver()
            });
        }

        public async Task<List<Showcase>> GetShowcases()
        {
            var url = string.Format(ShowcaseUrl, string.Empty);
            var data = await BlobCache.LocalMachine.DownloadUrl(url, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
            return JsonConvert.DeserializeObject<List<Showcase>>(Encoding.UTF8.GetString(data, 0, data.Length), new JsonSerializerSettings {
                ContractResolver = new UnderscoreContractResolver()
            });
        }
    }
}

