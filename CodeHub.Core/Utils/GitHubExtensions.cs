using System;
using GitHubSharp.Models;
using System.Text;
using System.Security.Cryptography;


public static class GitHubExtensions
{
    private const string GitHubDefaultGravitar = "https%3A%2F%2Fassets-cdn.github.com%2Fimages%2Fgravatars%2Fgravatar-user-420.png&r=x&s=140";

    public static string GenerateCommiterName(this CommitModel x)
    {
        if (x.Commit.Author != null && !string.IsNullOrEmpty(x.Commit.Author.Name))
            return x.Commit.Author.Name;
        if (x.Commit.Committer != null && !string.IsNullOrEmpty(x.Commit.Committer.Name))
            return x.Commit.Committer.Name;
        if (x.Author != null)
            return x.Author.Login;
        return x.Committer != null ? x.Committer.Login : "Unknown";
    }

    public static Uri GenerateGravatarUrl(this CommitModel x)
    {
        if (x == null)
            return null;

        try
        {
            if (x.Author != null && !string.IsNullOrEmpty(x.Author.AvatarUrl))
                return new Uri(x.Author.AvatarUrl);

            var inputBytes = Encoding.UTF8.GetBytes(x.Commit.Author.Email.Trim().ToLower());
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

    