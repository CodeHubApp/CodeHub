// Analysis disable once CheckNamespace
namespace System
{
    public static class StringExtensions
    {
        public static bool ContainsKeyword(this string @this, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return true;
            if (string.IsNullOrEmpty(@this))
                return false;
            return @this.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

