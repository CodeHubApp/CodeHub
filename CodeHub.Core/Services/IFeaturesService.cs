using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface IFeaturesService
    {
        bool IsPushNotificationsActivated { get; set; }

        void Activate(string id);
        bool IsActivated(string id);
    }
}

