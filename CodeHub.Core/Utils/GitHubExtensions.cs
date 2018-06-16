using System;
using System.Text;
using System.Security.Cryptography;

public static class GitHubExtensions
{
    private const string GitHubDefaultGravitar = "https%3A%2F%2Fassets-cdn.github.com%2Fimages%2Fgravatars%2Fgravatar-user-420.png&r=x&s=140";

    public static Uri GenerateCommiterAvatarUrl(this Octokit.GitHubCommit x)
    {
        try
        {
            var avatarUrl = x.Author?.AvatarUrl;
            if (!string.IsNullOrEmpty(avatarUrl))
                return new Uri(avatarUrl);
            return GenerateGravatarUrl(x.Commit.Author.Email);
        }
        catch
        {
            return null;
        }
    }

    public static Uri GenerateCommiterAvatarUrl(this Octokit.PullRequestCommit x)
    {
        try
        {
            var avatarUrl = x.Commit?.User?.AvatarUrl;
            if (!string.IsNullOrEmpty(avatarUrl))
                return new Uri(avatarUrl);
            return GenerateGravatarUrl(x.Commit.Author.Email);
        }
        catch
        {
            return null;
        }
    }

    public static Uri GenerateGravatarUrl(string email)
    {
        var inputBytes = Encoding.UTF8.GetBytes(email.Trim().ToLower());
        var hash = MD5.Create().ComputeHash(inputBytes);
        var sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
            sb.Append(hash[i].ToString("x2"));
        return new Uri(string.Format("http://www.gravatar.com/avatar/{0}?d={1}", sb, GitHubDefaultGravitar));
    }
}

    