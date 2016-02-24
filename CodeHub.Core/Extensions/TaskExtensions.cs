using System;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive;

// Analysis disable once CheckNamespace
public static class TaskExtensions
{
    public static IDisposable ToBackground<T>(this Task<T> task, Action<T> action)
    {
        return task.ToObservable()
            .Catch(Observable.Empty<T>())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(action, e => System.Diagnostics.Debug.WriteLine("Unable to process background task: " + e.Message));
    }

    public static IDisposable ToBackground<T>(this Task<T> task)
    {
        return task.ToObservable()
            .Catch(Observable.Empty<T>())
            .Subscribe(a => {}, e => System.Diagnostics.Debug.WriteLine("Unable to process background task: " + e.Message));
    }

    public static IDisposable ToBackground(this Task task)
    {
        return task.ToObservable()
            .Catch(Observable.Empty<Unit>())
            .Subscribe(a => {}, e => System.Diagnostics.Debug.WriteLine("Unable to process background task: " + e.Message));
    }
}


