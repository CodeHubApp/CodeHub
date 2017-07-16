using System;
using System.Collections.Generic;
using CodeHub.Core.Utils;

namespace Octokit
{
    public static class GitHubClientExtensions
    {
        public static GitHubList<T> RetrieveList<T>(
            this GitHubClient client,
            Uri uri,
            IDictionary<string, string> parameters = null)
        {
            return new GitHubList<T>(client, uri, parameters);
        }
    }
}
