using System;
using System.Text;
using System.Security.Cryptography;

public static class GitHubExtensions
{
    private const string GitHubDefaultGravitar = "https%3A%2F%2Fassets-cdn.github.com%2Fimages%2Fgravatars%2Fgravatar-user-420.png&r=x&s=140";

    public static string GenerateCommiterName(this Octokit.GitHubCommit x)
    {
        if (!string.IsNullOrEmpty(x.Author?.Login))
            return x.Author?.Login;
        if (!string.IsNullOrEmpty(x.Commit.Author?.Name))
            return x.Commit.Author.Name;
        if (!string.IsNullOrEmpty(x.Committer?.Login))
            return x.Committer.Login;
        if (!string.IsNullOrEmpty(x.Commit.Committer?.Name))
            return x.Commit.Committer.Name;
        return "Unknown";
    }

    public static Uri GenerateGravatarUrl(this Octokit.GitHubCommit x)
    {
        if (x == null)
            return null;

        if (!string.IsNullOrEmpty(x.User?.AvatarUrl))
            return new Uri(x.User.AvatarUrl);

        var email = x.Commit?.Author?.Email?.Trim().ToLower();
        if (string.IsNullOrEmpty(email))
            return null;

        try
        {
            var inputBytes = Encoding.UTF8.GetBytes(email);
            var hash = MD5.Create().ComputeHash(inputBytes);
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return new Uri(string.Format("http://www.gravatar.com/avatar/{0}?d={1}", sb, GitHubDefaultGravitar));
        }
        catch
        {
            return null;
        }
    }
}

    