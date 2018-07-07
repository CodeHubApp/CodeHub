using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace CodeHub.Core.Utils
{
    public interface IDataRetriever<T>
    {
        bool HasMore { get; }

        void Reset();

        Task<IReadOnlyList<T>> Next();
    }

    public static class StaticList
    {
        public static StaticList<T> From<T>(IEnumerable<T> list)
            => new StaticList<T>(list);
    }

    public class StaticList<T> : IDataRetriever<T>
    {
        private readonly IReadOnlyList<T> _list;

        public static StaticList<T> From<T>(IEnumerable<T> list)
            => new StaticList<T>(list);
        
        public StaticList(IEnumerable<T> list)
        {
            _list = list.ToList().AsReadOnly();
        }

        public bool HasMore => false;

        public Task<IReadOnlyList<T>> Next() => Task.FromResult(_list);

        public void Reset()
        {
        }
    }

    public class GitHubList<T> : IDataRetriever<T>
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
                return null;

            var ret = await _client.Connection.Get<IReadOnlyList<T>>(
                _currentUri, _parameters, "application/json");

            _currentUri = ret.HttpResponse.ApiInfo.Links.ContainsKey("next")
				? ret.HttpResponse.ApiInfo.Links["next"]
				: null;

            return ret.Body;
        }
    }
}
