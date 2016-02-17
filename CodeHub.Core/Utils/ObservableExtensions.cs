using System;
using System.Windows.Input;
using System.Reactive.Disposables;

namespace System
{
    public static class ObservableExtensions
    {
        public static IDisposable BindCommand<T>(this IObservable<T> @this, ICommand command)
        {
            return command == null ? Disposable.Empty : @this.Subscribe(x => command.Execute(x));
        }
    }
}

