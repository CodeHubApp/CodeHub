using System;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public class GitHubAccountsService : AccountsService<GitHubAccount>
    {
        public GitHubAccountsService(IDefaultValueService defaults, IAccountPreferencesService accountPreferences) 
            : base(defaults, accountPreferences)
        {
            Console.WriteLine("Accounts created!");
        }
    }
}
