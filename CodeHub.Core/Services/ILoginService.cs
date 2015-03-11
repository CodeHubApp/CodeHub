using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface ILoginService
    {
		Task<LoginData> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain, GitHubAccount existingAccount);

		Task<GitHubSharp.Client> LoginAccount(GitHubAccount account);

        Task<GitHubAccount> Authenticate(string apiDomain, string webDomain, string user, string pass, string twoFactor, bool enterprise);

        Task<GitHubAccount> Authenticate(string apiDomain, string webDomain, string token, bool enterprise);
    }

	public class LoginData
	{
		public GitHubSharp.Client Client { get; set; }
		public GitHubAccount Account { get; set; }
	}
}