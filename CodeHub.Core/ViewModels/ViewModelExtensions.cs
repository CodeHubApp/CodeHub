using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;
using Splat;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels
{
    public static class ViewModelExtensions
    {
        public static T CreateViewModel<T>(this IBaseViewModel @this)
        {
            return Locator.Current.GetService<IServiceConstructor>().Construct<T>();
        }

        public static async Task RequestModel<TRequest>(this object viewModel, GitHubRequest<TRequest> request, bool? forceDataRefresh, Action<GitHubResponse<TRequest>> update) where TRequest : new()
        {
            var force = forceDataRefresh.HasValue && forceDataRefresh.Value;
            if (force)
            {
                request.CheckIfModified = false;
                request.RequestFromCache = false;
            }

            var application = Locator.Current.GetService<IApplicationService>();

            var result = await application.Client.ExecuteAsync(request);
            update(result);

            if (result.WasCached)
            {
                request.RequestFromCache = false;
                Observable.FromAsync(() => application.Client.ExecuteAsync(request))
                    .ObserveOn(RxApp.MainThreadScheduler).Subscribe(update);
            }
        }

        public static void CreateMore<T>(this object viewModel, GitHubResponse<List<T>> response, 
            Action<Func<Task>> assignMore, Action<List<T>> newDataAction) where T : new()
        {
            if (response.More == null)
            {
                assignMore(null);
                return;
            }

            assignMore(async () =>
            {
                response.More.UseCache = false;
                var moreResponse = await Locator.Current.GetService<IApplicationService>().Client.ExecuteAsync(response.More);
                viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                newDataAction(moreResponse.Data);
            });
        }

        public static Task SimpleCollectionLoad<T>(this IReactiveList<T> viewModel, GitHubRequest<List<T>> request, bool? forceDataRefresh, Action<Func<Task>> assignMore = null) where T : new()
        {
            if (assignMore == null)
                assignMore = (x) => {};

            return viewModel.RequestModel(request, forceDataRefresh, response =>
            {
                viewModel.CreateMore(response, assignMore, x =>
                {
                    // This is fucking broken for iOS because it can't handle estimated rows and the insertions
                    // that ReactiveUI seems to be producing
                    using (viewModel.SuppressChangeNotifications())
                    {
                        viewModel.AddRange(x);
                    }
                });
                viewModel.Reset(response.Data);
            });
        }
    }
}

