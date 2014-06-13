namespace CodeHub.Core.Messages
{
	public class NotificationCountMessage 
	{
	    public NotificationCountMessage(int count)
	    {
	        Count = count;
	    }

	    public int Count { get; private set; }
	}
}

