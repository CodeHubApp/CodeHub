using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace CodeHub.Core.Utils
{
    public class GitHubList<T>
    {
        private readonly GitHubClient _client;
        private readonly IDictionary<string, string> _parameters;
        private readonly Uri _uri;
        private Uri _currentUri;

        public GitHubList(
            GitHubClient client,
            Uri uri,
            IDictionary<string, string> parameters = null)
        {
            _client = client;
            _uri = uri;
            _currentUri = uri;
            _parameters = parameters;
        }

        public bool HasMore => _currentUri != null;

        public void Reset() => _currentUri = _uri;

        public async Task<IReadOnlyList<T>> Next() 
        {
            if (!HasMore)
                return new List<T>().AsReadOnly();

            var ret = await _client.Connection.Get<IReadOnlyList<T>>(
                _currentUri, _parameters, "application/json");

            _currentUri = ret.HttpResponse.ApiInfo.Links.ContainsKey("next")
				? ret.HttpResponse.ApiInfo.Links["next"]
				: null;

            return ret.Body;
        }
    }
}
