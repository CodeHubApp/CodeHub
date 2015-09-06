namespace CodeFramework.Core.Services
{
    public interface IUIThreadService
    {
		void MarshalOnUIThread(System.Action a);
    }
}

