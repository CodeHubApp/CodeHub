using System;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
    {
        public static IDisposable ExecuteNow<TParam, TResult>(
            this ReactiveCommand<TParam, TResult> cmd, TParam param = default(TParam))
            => cmd.CanExecute.Take(1).Where(x => x).Select(_ => param).InvokeReactiveCommand(cmd);

        public static IDisposable InvokeReactiveCommand<TParam, TResult>(
            this IObservable<TParam> obs, ReactiveCommand<TParam, TResult> cmd)
            => obs.InvokeCommand(cmd);

        public static IDisposable InvokeReactiveCommand<TParam, TResult>(
            this IObservable<TParam> obs, CombinedReactiveCommand<TParam, TResult> cmd)
            => obs.InvokeCommand(cmd);
    }
}