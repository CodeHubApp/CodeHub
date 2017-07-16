using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace CodeHub.Core.Utils
{
    public class GitHubList<T>
    {
        private readonly GitHubClient _client;
        private Uri _uri;
        private readonly IDictionary<string, string> _parameters;

        public GitHubList(
            GitHubClient client,
            Uri uri,
            IDictionary<string, string> parameters = null)
        {
            _client = client;
            _uri = uri;
            _parameters = parameters;
        }

        public async Task<IReadOnlyList<T>> Next() 
        {
            if (_uri == null)
                return null;

            var ret = await _client.Connection.Get<IReadOnlyList<T>>(
                _uri, _parameters, "application/json");

            _uri = ret.HttpResponse.ApiInfo.Links.ContainsKey("next")
				? ret.HttpResponse.ApiInfo.Links["next"]
				: null;

            return ret.Body;
        }
    }
}
