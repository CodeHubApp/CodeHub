namespace CodeHub.Core.Services
{
    public interface IShareService
    {
        void ShareUrl(System.Uri uri);

        void OpenWith(System.Uri uri);
    }
}

