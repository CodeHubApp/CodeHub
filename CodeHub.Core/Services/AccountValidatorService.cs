using System.Threading.Tasks;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public class AccountValidatorService : IAccountValidatorService
    {
        private readonly ILoginService _loginFactory;

        public AccountValidatorService(ILoginService loginFactory)
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