using CodeFramework.Core.Data;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IApplicationService
    {
        GitHubSharp.Client Client { get; }
 
        GitHubAccount Account { get; }

        IAccountsService<IAccount> Accounts { get; }
    }
}