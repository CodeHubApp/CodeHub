namespace CodeHub.Core.Services
{
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
    }
}

