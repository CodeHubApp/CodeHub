using System;
using System.Reactive.Disposables;

namespace CodeHub.Core.Services
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

