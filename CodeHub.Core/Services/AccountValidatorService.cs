using System.Threading.Tasks;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;
using CodeHub.Core.Factories;

namespace CodeHub.Core.Services
{
    public class AccountValidatorService : IAccountValidatorService
    {
        private readonly ILoginFactory _loginFactory;

        public AccountValidatorService(ILoginFactory loginFactory)
        {
            _loginFactory = loginFactory;
        }

        public async Task Validate(CodeFramework.Core.Data.IAccount account)
        {
            var githubAccount = (GitHubAccount)account;
            await _loginFactory.LoginAccount(githubAccount);
        }
    }

}