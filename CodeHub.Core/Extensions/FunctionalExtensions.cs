using System;

// Analysis disable once CheckNamespace
public static class FunctionalExtensions
{
    public static TResult With<TSource, TResult>(this TSource source, Func<TSource, TResult> selector, Func<TResult> nullselector = null) 
    {
        if (source != null && selector != null)
            return selector(source);
        return nullselector != null ? nullselector() : default(TResult);
    }

    public static void Do<TSource>(this TSource source, Action<TSource> selector) 
    {
        if (source == null)
            return;
        selector(source);
    }
}

