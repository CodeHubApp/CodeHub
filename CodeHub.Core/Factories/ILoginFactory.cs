using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.Factories
{
    public interface ILoginFactory
    {
        Task<LoginData> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain);

        Task<GitHubSharp.Client> LoginAccount(GitHubAccount account);

        Task<GitHubAccount> LoginWithBasic(string domain, string user, string pass, string twoFactor = null);
    }

    public class LoginData
    {
        public GitHubSharp.Client Client { get; set; }
        public GitHubAccount Account { get; set; }
    }
}