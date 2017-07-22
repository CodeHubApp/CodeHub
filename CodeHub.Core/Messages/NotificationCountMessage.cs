namespace CodeHub.Core.Messages
{
    public class NotificationCountMessage
    {
        public int Count { get; }

        public NotificationCountMessage(int count)
        {
            Count = count;
        }
    }
}

