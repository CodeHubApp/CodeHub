namespace CodeHub.Core.Services
{
    public interface IUIThreadService
    {
		void MarshalOnUIThread(System.Action a);
    }
}

