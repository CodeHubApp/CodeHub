using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface ILoginService
    {
        GitHubSharp.Client LoginWithToken(string accessToken);

        GitHubSharp.Client LoginAccount(GitHubAccount account);

        GitHubAccount Authenticate(string domain, string user, string pass, string twoFactor);

        bool NeedsAuthentication(GitHubAccount account);
    }
}