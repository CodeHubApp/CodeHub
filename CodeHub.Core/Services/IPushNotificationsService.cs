using System;

namespace CodeHub.Core.Services
{
    public interface IPushNotificationsService
    {
		void Register();

		void Deregister();
    }
}

