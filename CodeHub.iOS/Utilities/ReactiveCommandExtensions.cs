using System;
using UIKit;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive;

// Analysis disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
    {
        public static IDisposable ToBarButtonItem(this IObservable<ReactiveCommand> @this, UIImage image, Action<UIBarButtonItem> assignment)
        {
            return ToBarButtonItem(@this, () => new UIBarButtonItem { Image = image }, assignment);
        }

        public static IDisposable ToBarButtonItem(this IObservable<ReactiveCommand> @this, UIBarButtonSystemItem systemItem, Action<UIBarButtonItem> assignment)
        {
            return ToBarButtonItem(@this, () => new UIBarButtonItem(systemItem), assignment);
        }

        public static IDisposable ToBarButtonItem(this IObservable<ReactiveCommand> @this, Func<UIBarButtonItem> creator, Action<UIBarButtonItem> assignment)
        {
            var unassignDisposable = Disposable.Create(() => assignment(null));
            IDisposable recentEventDisposable = Disposable.Empty;

            var mainDisposable = @this.Subscribe(x => {
                recentEventDisposable?.Dispose();

                var button = creator();
                var canExecuteDisposable = x.CanExecute.Subscribe(t => button.Enabled = t);
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

