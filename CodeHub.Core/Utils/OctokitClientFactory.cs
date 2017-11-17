using System;
using Octokit.Internal;
using Splat;
using Octokit;
using System.Net.Http;
using CodeHub.Core.Services;

namespace CodeHub.Core.Utilities
{
    public static class OctokitClientFactory
    {
        public static Func<HttpClientHandler> CreateMessageHandler = () => new HttpClientHandler();
        public static readonly string[] Scopes = { "user", "repo", "gist", "notifications" };
        public static readonly ProductHeaderValue UserAgent = new ProductHeaderValue("CodeHub");

        public static GitHubClient Create(
            Uri domain,
            Credentials credentials,
            TimeSpan? requestTimeout = null)
        {
            var networkActivityService = Locator.Current.GetService<INetworkActivityService>();
            var client = new HttpClientAdapter(CreateMessageHandler);
            var httpClient = new OctokitNetworkClient(client, networkActivityService);

            var connection = new Connection(
                UserAgent,
                domain,
                new InMemoryCredentialStore(credentials),
                httpClient,
                new SimpleJsonSerializer());
            
            var gitHubClient = new GitHubClient(connection);
            gitHubClient.SetRequestTimeout(requestTimeout ?? TimeSpan.FromSeconds(20));
            return gitHubClient;
        }
    }
}

