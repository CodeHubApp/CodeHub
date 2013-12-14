using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface ILoginService
    {
		Task<LoginData> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain, GitHubAccount existingAccount);

		Task<GitHubSharp.Client> LoginAccount(GitHubAccount account);

		LoginData Authenticate(string domain, string user, string pass, string twoFactor, bool enterprise, GitHubAccount existingAccount);
    }

	public class LoginData
	{
		public GitHubSharp.Client Client { get; set; }
		public GitHubAccount Account { get; set; }
	}
}