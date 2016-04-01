using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using UIKit;


// ReSharper disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
    {
        public static void ExecuteIfCan(this ICommand @this, object o)
        {
            if (@this == null)
                return;

            if (@this.CanExecute(o))
                @this.Execute(o);
        }

        public static void ExecuteIfCan(this ICommand @this)
        {
            ExecuteIfCan(@this, null);
        }

        public static IReactiveCommand<T> WithSubscription<T>(this IReactiveCommand<T> @this, Action<T> action)
        {
            @this.Subscribe(action);
            return @this;
        }

        public static IDisposable ToBarButtonItem(this IObservable<IReactiveCommand> @this, UIImage image, Action<UIBarButtonItem> assignment)
        {
            return ToBarButtonItem(@this, () => new UIBarButtonItem { Image = image }, assignment);
        }

        public static IDisposable ToBarButtonItem(this IObservable<IReactiveCommand> @this, UIBarButtonSystemItem systemItem, Action<UIBarButtonItem> assignment)
        {
            return ToBarButtonItem(@this, () => new UIBarButtonItem(systemItem), assignment);
        }

        public static IDisposable ToBarButtonItem(this IObservable<IReactiveCommand> @this, Func<UIBarButtonItem> creator, Action<UIBarButtonItem> assignment)
        {
            var unassignDisposable = Disposable.Create(() => assignment(null));
            IDisposable recentEventDisposable = Disposable.Empty;

            var mainDisposable = @this.Subscribe(x => {
                recentEventDisposable?.Dispose();

                var button = creator();
                var canExecuteDisposable = x.CanExecuteObservable.Subscribe(t => button.Enabled = t);
                var clickDisposable = Observable.FromEventPattern(t => button.Clicked += t, t => button.Clicked -= t)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(x);

                recentEventDisposable = new CompositeDisposable(clickDisposable, canExecuteDisposable);
                assignment(button);
            });

            return new CompositeDisposable(mainDisposable, unassignDisposable, Disposable.Create(() => recentEventDisposable.Dispose()));
        }
    }
}