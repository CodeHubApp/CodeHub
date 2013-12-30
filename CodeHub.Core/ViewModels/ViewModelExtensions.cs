using System;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using CodeFramework.Core.Services;

namespace CodeHub.Core.ViewModels
{
    public static class ViewModelExtensions
    {
        public static Task RequestModel<TRequest>(this MvxViewModel viewModel, GitHubRequest<TRequest> request, bool forceDataRefresh, Action<GitHubResponse<TRequest>> update) where TRequest : new()
        {
            if (forceDataRefresh)
            {
                request.CheckIfModified = false;
                request.RequestFromCache = false;
            }

            var application = Mvx.Resolve<IApplicationService>();
            var uiThrad = Mvx.Resolve<IUIThreadService>();

			return Task.Run(async () =>
            {
				var result = await application.Client.ExecuteAsync(request).ConfigureAwait(false);
                uiThrad.MarshalOnUIThread(() => update(result));

                if (result.WasCached)
                {
                    request.RequestFromCache = false;

					Task.Run(async () =>
                    {
                        try
                        {
							var r = await application.Client.ExecuteAsync(request).ConfigureAwait(false);
                            uiThrad.MarshalOnUIThread(() => update(r));
                        }
						catch (NotModifiedException)
						{
							System.Diagnostics.Debug.WriteLine("Not modified: " + request.Url);
						}
                    }).FireAndForget();
                }
            });
		}

        public static void CreateMore<T>(this MvxViewModel viewModel, GitHubResponse<T> response, 
                                               Action<Task> assignMore, Action<T> newDataAction) where T : new()
        {
            if (response.More == null)
            {
                assignMore(null);
                return;
            }

            var task = new Task(async () =>
            {
                response.More.UseCache = false;
                var moreResponse = await Mvx.Resolve<IApplicationService>().Client.ExecuteAsync(response.More);
                viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                newDataAction(moreResponse.Data);
            });

            assignMore(task);
        }

        public static Task SimpleCollectionLoad<T>(this CollectionViewModel<T> viewModel, GitHubRequest<List<T>> request, bool forceDataRefresh) where T : new()
        {
            return viewModel.RequestModel(request, forceDataRefresh, response =>
            {
                viewModel.CreateMore(response, m => viewModel.MoreItems = m, viewModel.Items.AddRange);
                viewModel.Items.Reset(response.Data);
            });
        }
    }
}

