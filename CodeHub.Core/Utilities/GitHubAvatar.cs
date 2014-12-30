using System;

namespace CodeHub.Core.Utilities
{
    public class GitHubAvatar
    {
        private readonly string _avatarUrl;

        public GitHubAvatar(string avatarUrl)
        {
            _avatarUrl = avatarUrl;
        }

        public Uri ToUri(int? size = null)
        {
            if (_avatarUrl == null)
                return null;

            try
            {
                var baseUri = new UriBuilder(_avatarUrl);

                if (size == null)
                    return baseUri.Uri;

                var queryToAppend = "size=" + size.Value;
                if (baseUri.Query != null && baseUri.Query.Length > 1)
                    baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend; 
                else
                    baseUri.Query = queryToAppend; 
                return baseUri.Uri;
            }
            catch
            {
                return null;
            }
        }
    }
}

