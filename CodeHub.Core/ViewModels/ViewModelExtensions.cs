using System;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels
{
    public static class ViewModelExtensions
    {
		public static async Task RequestModel<TRequest>(this MvxViewModel viewModel, GitHubRequest<TRequest> request, bool forceDataRefresh, Action<GitHubResponse<TRequest>> update) where TRequest : new()
        {
            if (forceDataRefresh)
            {
                request.CheckIfModified = false;
                request.RequestFromCache = false;
            }

			var response = await Mvx.Resolve<IApplicationService>().Client.ExecuteAsync(request);
            update(response);

            if (response.WasCached)
            {
				Task.Run(async () => {
                    try
                    {
                        request.RequestFromCache = false;
						var r = await Mvx.Resolve<IApplicationService>().Client.ExecuteAsync(request);
						update(r);
                    }
                    catch (NotModifiedException)
                    {
                        Console.WriteLine("Not modified: " + request.Url);
                    }
					catch (Exception)
                    {
                        Console.WriteLine("SHIT! " + request.Url);
                    }
                });
            }
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
			return viewModel.RequestModel(request, forceDataRefresh, response => {
				viewModel.CreateMore(response, m => viewModel.MoreItems = m, viewModel.Items.AddRange);
                viewModel.Items.Reset(response.Data);
            });
        }
    }
}

