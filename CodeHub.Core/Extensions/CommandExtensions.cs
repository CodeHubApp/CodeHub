using System;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
    {
        public static IDisposable ExecuteNow<TParam, TResult>(this ReactiveCommand<TParam, TResult> cmd, TParam param = default(TParam))
        {
            return cmd.CanExecute.Take(1).Where(x => x).Select(_ => param).InvokeCommand(cmd);
        }
    }
}