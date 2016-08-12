using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.Services;
using System.ComponentModel;
using System.Collections.Specialized;
using MvvmCross.Platform;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Disposables;

namespace CodeHub.Core.ViewModels
{
    public static class ViewModelExtensions
    {
        public static async Task RequestModel<TRequest>(this MvxViewModel viewModel, GitHubRequest<TRequest> request, Action<GitHubResponse<TRequest>> update) where TRequest : new()
        {
            var application = Mvx.Resolve<IApplicationService>();
            var result = await application.Client.ExecuteAsync(request);
            update(result);
        }

        public static void CreateMore<T>(this MvxViewModel viewModel, GitHubResponse<T> response, 
                                         Action<Action> assignMore, Action<T> newDataAction) where T : new()
        {
            if (response.More == null)
            {
                assignMore(null);
                return;
            }

            Action task = () =>
            {
                var moreResponse = Mvx.Resolve<IApplicationService>().Client.ExecuteAsync(response.More).Result;
                viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                newDataAction(moreResponse.Data);
            };

            assignMore(task);
        }

        public static Task SimpleCollectionLoad<T>(this CollectionViewModel<T> viewModel, GitHubRequest<List<T>> request) where T : new()
        {
            var weakVm = new WeakReference<CollectionViewModel<T>>(viewModel);
            return viewModel.RequestModel(request, response =>
            {
                weakVm.Get()?.CreateMore(response, m => {
                    var weak = weakVm.Get();
                    if (weak != null)
                        weak.MoreItems = m;
                }, viewModel.Items.AddRange);
                weakVm.Get()?.Items.Reset(response.Data);
            });
        }
    }
}

public static class BindExtensions
{
    public static IObservable<TR> Bind<T, TR>(this T viewModel, System.Linq.Expressions.Expression<Func<T, TR>> outExpr, bool activate = false) where T : INotifyPropertyChanged
    {
        var expr = (System.Linq.Expressions.MemberExpression) outExpr.Body;
        var prop = (System.Reflection.PropertyInfo) expr.Member;
        var name = prop.Name;
        var comp = outExpr.Compile();

        var ret = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(t => viewModel.PropertyChanged += t, t => viewModel.PropertyChanged -= t)
            .Where(x => string.Equals(x.EventArgs.PropertyName, name))
            .Select(x => comp(viewModel));

        if (!activate)
            return ret;

        var o = Observable.Create<TR>(obs => {
            try
            {
                obs.OnNext(comp(viewModel));
            }
            catch (Exception e)
            {
                obs.OnError(e);
            }

            obs.OnCompleted();

            return Disposable.Empty;
        });

        return o.Concat(ret);
    }

    public static IObservable<Unit> BindCollection<T>(this T viewModel, System.Linq.Expressions.Expression<Func<T, INotifyCollectionChanged>> outExpr, bool activate = false) where T : INotifyPropertyChanged
    {
        var exp = outExpr.Compile();
        var m = exp(viewModel);

        var ret = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(t => m.CollectionChanged += t, t => m.CollectionChanged -= t)
            .Select(_ => Unit.Default);
        return activate ? ret.StartWith(Unit.Default) : ret;
    }
}