using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeHub.Core.Utils
{
    public class GitHubPaginator<T> : IPaginator<T>
    {
        private readonly Octokit.GitHubClient _client;
        private readonly Uri _originalUri;
        private readonly IDictionary<string, string> _parameters;
        private Uri _currentUri;

        public GitHubPaginator(
            Octokit.GitHubClient client,
            Uri uri,
            IDictionary<string, string> parameters = null)
        {
            _client = client;
            _originalUri = uri;
            _parameters = parameters;
            _currentUri = uri;

            HasMore = true;
        }

        public bool HasMore { get; private set; }

        public async Task<IReadOnlyList<T>> Next()
        {
            try
            {
                var ret = await _client.Connection.Get<IReadOnlyList<T>>(_currentUri, _parameters, "application/json");
                _currentUri = ret.HttpResponse.ApiInfo.Links.ContainsKey("next")
                                 ? ret.HttpResponse.ApiInfo.Links["next"]
                                 : null;
                HasMore = _currentUri != null;
                return ret.Body;
            }
            catch
            {
                HasMore = false;
                throw;
            }
        }

        public void Reset()
        {
            _currentUri = _originalUri;
        }
    }

    public interface IPaginator<T>
    {
        bool HasMore { get; }

        void Reset();

        Task<IReadOnlyList<T>> Next();
    }
}
