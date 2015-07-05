using CodeHub.Core.ViewModels;

namespace CodeHub.Core.Services
{
    public interface IPushNotificationService
    {
        PushNotificationAction Handle(PushNotificationRequest command);
    }
}

