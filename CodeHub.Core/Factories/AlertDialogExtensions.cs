using System;
using System.Reactive.Disposables;
using CodeHub.Core.Factories;

namespace CodeHub.Core.Factories
{
    public static class AlertDialogExtensions
    {
        public static IDisposable Activate(this IAlertDialogFactory @this, string text)
        {
            @this.Show(text);
            return Disposable.Create(@this.Hide);
        }
    }
}

