// Analysis disable once CheckNamespace
namespace System
{
    using System;
    using System.Windows.Input;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

    public static class ObservableExtensions
    {
        public static IDisposable BindCommand<T>(this IObservable<T> @this, ICommand command)
        {
            return command == null ? Disposable.Empty : @this.Where(x => command.CanExecute(x)).Subscribe(x => command.Execute(x));
        }

        public static IDisposable SubscribeError<T>(this IObservable<T> @this, Action<Exception> onError)
        {
            return @this.Subscribe(_ => { }, onError);
        }
    }
}
