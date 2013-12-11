using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface ILoginService
    {
		Task<GitHubSharp.Client> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain);

		Task<GitHubSharp.Client> LoginAccount(GitHubAccount account);

        GitHubAccount Authenticate(string domain, string user, string pass, string twoFactor);

        bool NeedsAuthentication(GitHubAccount account);
    }
}