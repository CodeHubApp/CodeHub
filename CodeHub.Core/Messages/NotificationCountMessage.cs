using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class NotificationCountMessage : MvxMessage
    {
        public NotificationCountMessage(object sender) : base(sender) {}
        public int Count;
    }
}

