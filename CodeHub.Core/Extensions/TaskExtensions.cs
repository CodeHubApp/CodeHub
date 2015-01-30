using System;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using ReactiveUI;

// Analysis disable once CheckNamespace
using System.Reactive;
using System.Reactive.Subjects;


public static class TaskExtensions
{
    public static IDisposable ToBackground<T>(this Task<T> task, Action<T> action)
    {
        return task.ToObservable()
            .Catch(Observable.Empty<T>())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(action, e => System.Diagnostics.Debug.WriteLine("Unable to process background task: " + e.Message));
    }

    public static IDisposable ToBackground<T>(this Task<T> task, ISubject<T> subject)
    {
        return task.ToObservable()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(subject.OnNext, subject.OnError, subject.OnCompleted);
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


