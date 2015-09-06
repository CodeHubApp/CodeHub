using System;
using System.Threading.Tasks;

namespace CodeHub.Core.Factories
{
    public interface IFeatureFactory
    {
        Task PromptPushNotificationFeature();
    }
}

