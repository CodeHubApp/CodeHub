using System;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using ReactiveUI;

// Analysis disable once CheckNamespace
public static class TaskExtensions
{
    public static IDisposable ToBackground<T>(this Task<T> task, Action<T> action)
    {
        return task.ToObservable()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(action, e => System.Diagnostics.Debug.WriteLine("Unable to process background task: " + e.Message));
    }
}


