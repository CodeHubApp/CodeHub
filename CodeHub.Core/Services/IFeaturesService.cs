using System.Threading.Tasks;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public interface IFeaturesService
    {
        bool IsPushNotificationsActivated { get; set; }

        bool IsEnterpriseSupportActivated { get; set; }

        void Activate(string id);

        bool IsActivated(string id);

        Task<IEnumerable<string>> GetAvailableFeatureIds();
    }

    public static class FeatureIds
    {
        public const string PushNotifications = "com.dillonbuchanan.codehub.push";
        public const string EnterpriseSupport = "com.dillonbuchanan.codehub.enterprise_support";
    }
}

