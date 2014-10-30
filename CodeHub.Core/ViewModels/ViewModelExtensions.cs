using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels
{
    public static class ViewModelExtensions
    {
        public static void ShowWebBrowser(this BaseViewModel @this, string url)
        {
            var vm = @this.CreateViewModel<WebBrowserViewModel>();
            vm.Url = url;
            @this.ShowViewModel(vm);
        }

        public static IReactiveCommand CreateUrlCommand(this BaseViewModel @this)
        {
            var command = ReactiveCommand.Create();
            command.OfType<string>().Subscribe(@this.ShowWebBrowser);
            return command;
        }

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
                var moreResponse = await IoC.Resolve<IApplicationService>().Client.ExecuteAsync(response.More);
                viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                newDataAction(moreResponse.Data);
            });
        }

        public static Task SimpleCollectionLoad<T>(this ReactiveList<T> viewModel, GitHubRequest<List<T>> request, bool? forceDataRefresh, Action<Func<Task>> assignMore = null) where T : new()
        {
            if (assignMore == null)
                assignMore = (x) => {};

            return viewModel.RequestModel(request, forceDataRefresh, response =>
            {
                viewModel.CreateMore(response, assignMore, x => 
                {
                    viewModel.AddRange(x);
                    Console.WriteLine("The size is: " + viewModel.Count);
                });
                viewModel.Reset(response.Data);
            });
        }

        public static async Task LoadAll<T>(this ReactiveList<T> @this, GitHubRequest<List<T>> request) where T : new()
        {
            var application = IoC.Resolve<IApplicationService>();
            @this.Clear();

            while (request != null)
            {
                request.RequestFromCache = false;
                var result = await application.Client.ExecuteAsync(request);
                if (@this.Count == 0)
                    @this.Reset(result.Data.Where(x => x != null));
                else
                    @this.AddRange(result.Data.Where(x => x != null));
                request = result.More;
            }
        }
    }
}

