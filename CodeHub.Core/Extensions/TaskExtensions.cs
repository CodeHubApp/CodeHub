using System;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using ReactiveUI;

// Analysis disable once CheckNamespace
public static class TaskExtensions
{
    public static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        return task.ToObservable().Timeout(timeout).ToTask();
    }

    public static Task WithTimeout(this Task task, TimeSpan timeout)
    {
        return task.ToObservable().Timeout(timeout).ToTask();
    }

    public static IDisposable ToBackground<T>(this Task<T> task, Action<T> action)
    {
        return task.ToObservable()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(action, HandleError);
    }

    public static IDisposable ToBackground<T>(this Task<T> task)
    {
        return task.ToObservable()
            .Subscribe(a => {}, HandleError);
    }

    public static IDisposable ToBackground(this Task task)
    {
        return task.ToObservable()
            .Subscribe(a => {}, HandleError);
    }

    public static IDisposable ToBackground(this Task task, Action action)
    {
        return task.ToObservable()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => action(), HandleError);
    }

    private static void HandleError(Exception e)
    {
        System.Diagnostics.Debug.WriteLine("Unable to process background task: " + e.Message);
    }
}


