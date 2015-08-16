using System;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.Utilities
{
    public class GitHubAvatar
    {
        public string AvatarUrl { get; }

        public static GitHubAvatar Empty
        {
            get { return new GitHubAvatar((string)null); }
        }

        public GitHubAvatar(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
        }

        public GitHubAvatar(Uri avatarUri)
        {
            AvatarUrl = avatarUri.AbsoluteUri;
        }
    }
}

public static class GitHubAvatarExtensions
{
    public static Uri ToUri(this GitHubAvatar @this, int? size = null)
    {
        if (@this == null || @this.AvatarUrl == null)
            return null;

        try
        {
            var baseUri = new UriBuilder(@this.AvatarUrl);

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

