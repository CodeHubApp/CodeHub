using System;
using System.Reactive.Disposables;
using CodeHub.Core.Factories;
using ReactiveUI;

namespace CodeHub.Core.Factories
{
    public static class AlertDialogExtensions
    {
        public static IDisposable Activate(this IAlertDialogFactory @this, string text)
        {
            @this.Show(text);
            return Disposable.Create(@this.Hide);
        }

        public static IDisposable Activate(this IAlertDialogFactory @this, IObservable<bool> observable, string text)
        {
            return observable.Subscribe(x => {
                if (x)
                    @this.Show(text);
                else
                    @this.Hide();
            });
        }

        public static IDisposable Activate(this IAlertDialogFactory @this, IReactiveCommand command, string text)
        {
            return command.IsExecuting.Subscribe(x => {
                if (x)
                    @this.Show(text);
                else
                    @this.Hide();
            });
        }

        public static IDisposable AlertExecuting(this IReactiveCommand @this, IAlertDialogFactory dialogFactory, string text)
        {
            return @this.IsExecuting.Subscribe(x => {
                if (x)
                    dialogFactory.Show(text);
                else
                    dialogFactory.Hide();
            });
        }
    }
}

