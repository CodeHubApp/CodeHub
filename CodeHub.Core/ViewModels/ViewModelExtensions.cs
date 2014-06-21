using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels
{
    public static class ViewModelExtensions
    {
        public static async Task RequestModel<TRequest>(this object viewModel, GitHubRequest<TRequest> request, bool? forceDataRefresh, Action<GitHubResponse<TRequest>> update) where TRequest : new()
        {
            var force = forceDataRefresh.HasValue && forceDataRefresh.Value;
            if (force)
            {
                request.CheckIfModified = false;
                request.RequestFromCache = false;
            }

            var application = IoC.Resolve<IApplicationService>();

            var result = await application.Client.ExecuteAsync(request);
            update(result);

            if (result.WasCached)
            {
                request.RequestFromCache = false;
                var uncachedTask = application.Client.ExecuteAsync(request);
                uncachedTask.ContinueInBackground(update);
            }
		}

        public static void CreateMore<T>(this object viewModel, GitHubResponse<List<T>> response, 
										 Action<Task> assignMore, Action<List<T>> newDataAction) where T : new()
        {
            if (response.More == null)
            {
                assignMore(null);
                return;
            }

            assignMore(new Task(() =>
            {
                response.More.UseCache = false;
                var moreResponse = IoC.Resolve<IApplicationService>().Client.ExecuteAsync(response.More).Result;
                viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                newDataAction(moreResponse.Data);
            }));
        }

        public static Task SimpleCollectionLoad<T>(this ReactiveCollection<T> viewModel, GitHubRequest<List<T>> request, bool? forceDataRefresh) where T : new()
        {
            return viewModel.RequestModel(request, forceDataRefresh, response =>
            {
                viewModel.CreateMore(response, m => viewModel.MoreTask = m, x => viewModel.AddRange(x));
                viewModel.Reset(response.Data);
            });
        }
    }
}

