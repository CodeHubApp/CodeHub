using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface IPushNotificationsService
    {
        Task Register();

        Task Deregister();
    }
}

