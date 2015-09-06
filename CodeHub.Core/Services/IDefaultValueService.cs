namespace CodeFramework.Core.Services
{
    public interface IDefaultValueService
    {
        T Get<T>(string key);

        bool TryGet<T>(string key, out T value);

        void Set(string key, object value);
    }
}