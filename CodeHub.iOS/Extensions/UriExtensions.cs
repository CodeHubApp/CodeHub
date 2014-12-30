using MonoTouch.Foundation;

// Analysis disable once CheckNamespace
namespace System
{
    public static class UriExtensions
    {
        public static NSUrl ToNSUrl(this Uri @this)
        {
            return @this == null ? null : new NSUrl(@this.AbsoluteUri);
        }
    }
}

