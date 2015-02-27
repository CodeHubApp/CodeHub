using System.Collections.Generic;

namespace CodeHub.Core
{
    public interface IStartupCommand
    {
    }

    public class PushNotificationCommand : IStartupCommand
    {
        public IDictionary<string, string> Attributes { get; private set; }

        public PushNotificationCommand(IDictionary<string, string> attributes)
        {
            Attributes = attributes;
        }
    }

    public class UrlCommand : IStartupCommand
    {
        public string Url { get; private set; }

        public UrlCommand(string url)
        {
            Url = url;
        }
    }
}

