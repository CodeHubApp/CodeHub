using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface IFeaturesService
    {
        bool IsPushNotificationsActivated { get; }

        bool IsEnterpriseSupportActivated { get; }

        bool IsPrivateRepositoriesEnabled { get; }

        bool IsProEnabled { get; }

        void ActivateProDirect();

        Task ActivatePro();

        Task RestorePro();
    }
}

