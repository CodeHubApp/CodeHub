using System;
using System.Windows.Input;

namespace System
{
    public static class ObservableExtensions
    {
        public static IDisposable BindCommand<T>(this IObservable<T> @this, ICommand command)
        {
            return @this.Subscribe(x => command.Execute(x));
        }
    }
}

