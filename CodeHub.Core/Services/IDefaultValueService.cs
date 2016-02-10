namespace CodeHub.Core.Services
{
    public interface IDefaultValueService
    {
        bool TryGet(string key, out string value);

        bool TryGet(string key, out int value);

        bool TryGet(string key, out bool value);

        void Set(string key, string value);

        void Set(string key, int? value);

        void Set(string key, bool? value);

        void Clear(string key);
    }
}