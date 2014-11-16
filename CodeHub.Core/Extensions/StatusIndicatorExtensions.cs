using System;
using Xamarin.Utilities.Core.Services;
using System.Reactive.Disposables;

namespace CodeHub.Core.Extensions
{
    public static class StatusIndicatorExtensions
    {
        public static IDisposable Activate(this IStatusIndicatorService @this, string text)
        {
            @this.Show(text);
            return Disposable.Create(@this.Hide);
        }
    }
}

