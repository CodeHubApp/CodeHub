using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using GitHubSharp;

namespace CodeHub.Core.Data
{
    public class LanguageRepository
    {
        public static Language DefaultLanguage = new Language("All Languages", null);
        private const string LanguagesUrl = "http://trending.codehub-app.com/v2/languages";

        public async Task<List<Language>> GetLanguages()
        {
            var client = new HttpClient();
            var serializer = new SimpleJsonSerializer();
            var msg = await client.GetAsync(LanguagesUrl).ConfigureAwait(false);
            var content = await msg.Content.ReadAsStringAsync().ConfigureAwait(false);
            return serializer.Deserialize<List<Language>>(content);
        }
    }
}

