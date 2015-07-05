using System.Threading.Tasks;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IPushNotificationRegistrationService
    {
        Task Register(GitHubAccount account);

        Task Deregister(GitHubAccount account);
    }
}

