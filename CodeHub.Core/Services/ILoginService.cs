using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface ILoginService
    {
        Task<GitHubAccount> Authenticate(string apiDomain, string webDomain, string user, string pass, string twoFactor, bool enterprise);

        Task<GitHubAccount> Authenticate(string apiDomain, string webDomain, string token, bool enterprise);
    }
}