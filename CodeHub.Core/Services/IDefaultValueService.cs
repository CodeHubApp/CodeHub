namespace CodeHub.Core.Services
{
    public interface IDefaultValueService
    {
        T Get<T>(string key);

        void Set(string key, object value);
    }

    public static class DefaultValueServiceExtensions
    {
        public static bool TryGet<T>(this IDefaultValueService @this, string key, out T value)
        {
            try
            {
                value = @this.Get<T>(key);
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }

        public static T GetOrDefault<T>(this IDefaultValueService @this, string key, T defaultValue)
        {
            T val;
            if (@this.TryGet(key, out val))
                return val;
            return defaultValue;
        }
    }
}