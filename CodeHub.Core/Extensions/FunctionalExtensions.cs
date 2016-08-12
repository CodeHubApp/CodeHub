using System;

// Analysis disable once CheckNamespace
public static class FunctionalExtensions
{
    public static void With<TSource>(this TSource source, Action<TSource> selector) 
    {
        if (source == null)
            return;
        selector(source);
    }
}

