using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface ILoginService
    {
        GitHubSharp.Client LoginWithToken(string accessToken);

		Task<GitHubSharp.Client> LoginAccount(GitHubAccount account);

        GitHubAccount Authenticate(string domain, string user, string pass, string twoFactor);

        bool NeedsAuthentication(GitHubAccount account);
    }
}