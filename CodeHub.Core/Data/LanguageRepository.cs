using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Akavache;
using System.Text;
using System.Reactive.Linq;

namespace CodeHub.Core.Data
{
    public class LanguageRepository
    {
        public static Language DefaultLanguage = new Language("All Languages", null);
        private const string LanguagesUrl = "http://trending.codehub-app.com/v2/languages";

        public async Task<List<Language>> GetLanguages()
        {
            var data = await BlobCache.LocalMachine.DownloadUrl(LanguagesUrl, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
            var serializer = new Octokit.Internal.SimpleJsonSerializer();
            var decodedData = Encoding.UTF8.GetString(data, 0, data.Length);
            return serializer.Deserialize<List<Language>>(decodedData);
        }
    }
}

