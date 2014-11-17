using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Akavache;
using Newtonsoft.Json;
using System.Text;
using CodeHub.Core.Utilities;
using System.Reactive.Linq;

namespace CodeHub.Core.Data
{
    public class LanguageRepository
    {
        public static Language DefaultLanguage = new Language { Name = "All Languages", Slug = null };
        private const string LanguagesUrl = "http://trending.codehub-app.com/v2/languages";

        public async Task<List<Language>> GetLanguages()
        {
            var trendingData = await BlobCache.LocalMachine.DownloadUrl(LanguagesUrl, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
            return JsonConvert.DeserializeObject<List<Language>>(Encoding.UTF8.GetString(trendingData), new JsonSerializerSettings {
                ContractResolver = new UnderscoreContractResolver()
            });
        }
    }
}

