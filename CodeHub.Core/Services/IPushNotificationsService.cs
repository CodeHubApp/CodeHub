using System;
using System.Threading.Tasks;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IPushNotificationsService
    {
        Task Register(GitHubAccount account);

        Task Deregister(GitHubAccount account);
    }
}

