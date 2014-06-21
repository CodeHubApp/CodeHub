using CodeFramework.Core.Services;
using CodeHub.Core.Data;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.Core.Services
{
    public class GitHubAccountsService : BaseAccountsService<GitHubAccount>
    {
        public GitHubAccountsService(IDefaultValueService defaults) 
            : base(defaults)
        {
        }
    }
}