using System;

public static class WeakReferenceExtensions
{
    public static T Get<T>(this WeakReference<T> @this) where T : class
    {
        T t;
        @this.TryGetTarget(out t);
        return t;
    }
}

