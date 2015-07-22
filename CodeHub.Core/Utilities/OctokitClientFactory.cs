using System;
using Octokit.Internal;
using Splat;
using CodeHub.Core.Services;
using Octokit;
using ModernHttpClient;

namespace CodeHub.Core.Utilities
{
    public static class OctokitClientFactory
    {
        public static Func<NativeMessageHandler> CreateMessageHandler = () => new NativeMessageHandler();
        public static readonly string[] Scopes = { "user", "repo", "gist", "notifications" };

        public static GitHubClient Create(Uri domain, Credentials credentials)
        {
            // Decorate the HttpClient
            //IHttpClient httpClient = new HttpClientAdapter();
            //httpClient = new OctokitCacheClient(httpClient);
            var client = new HttpClientAdapter(CreateMessageHandler);
            var httpClient = new OctokitNetworkClient(client, Locator.Current.GetService<INetworkActivityService>());

            var connection = new Connection(
                new ProductHeaderValue("CodeHub"),
                domain,
                new InMemoryCredentialStore(credentials),
                httpClient,
                new SimpleJsonSerializer());
            return new GitHubClient(connection);
        }
    }
}

