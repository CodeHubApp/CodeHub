using System.Collections.Generic;
using CodeHub.Core.ViewModels;

namespace CodeHub.Core
{
    public interface IStartupCommand
    {
    }

    public class PushNotificationRequest : IStartupCommand
    {
        public IDictionary<string, string> Attributes { get; private set; }

        public PushNotificationRequest(IDictionary<string, string> attributes)
        {
            Attributes = attributes;
        }
    }

    public class PushNotificationAction
    {
        public BaseViewModel ViewModel { get; private set; }

        public string Username { get; private set; }

        public PushNotificationAction(string username, BaseViewModel viewModel)
        {
            Username = username;
            ViewModel = viewModel;
        }
    }

    public class UrlStartupCommand : IStartupCommand
    {
        public string Url { get; private set; }

        public UrlStartupCommand(string url)
        {
            Url = url;
        }
    }
}

