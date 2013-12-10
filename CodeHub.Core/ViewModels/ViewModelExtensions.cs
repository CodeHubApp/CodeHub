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
        public static void RequestModel<TRequest>(this MvxViewModel viewModel, GitHubRequest<TRequest> request, bool forceDataRefresh, Action<GitHubResponse<TRequest>> update) where TRequest : new()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            if (forceDataRefresh)
            {
                request.CheckIfModified = false;
                request.RequestFromCache = false;
            }

            var response = Mvx.Resolve<IApplicationService>().Client.Execute(request);
            stopWatch.Stop();

            Console.WriteLine("Request executed in: " + stopWatch.ElapsedMilliseconds + "ms");

            stopWatch.Reset();
            stopWatch.Start();
            update(response);
            stopWatch.Stop();

            Console.WriteLine("View updated in: " + stopWatch.ElapsedMilliseconds + "ms");


            if (response.WasCached)
            {
                Task.Run(() => {
                    try
                    {
                        request.RequestFromCache = false;
                        update(Mvx.Resolve<IApplicationService>().Client.Execute(request));
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

        public static void CreateMore<T>(this MvxViewModel viewModel, 
                                         GitHubResponse<T> response, 
                                         Action<Action> assignMore, 
                                         Action<T> newDataAction) where T : new()
        {
            if (response.More == null)
            {
                assignMore(null);
                return;
            }

            assignMore(() => {
                                 response.More.UseCache = false;
                                 var moreResponse = Mvx.Resolve<IApplicationService>().Client.Execute(response.More);
                                 viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                                 newDataAction(moreResponse.Data);
            });
        }

        public static Task SimpleCollectionLoad<T>(this CollectionViewModel<T> viewModel, GitHubRequest<List<T>> request, bool forceDataRefresh) where T : new()
        {
            return Task.Run(() => viewModel.RequestModel(request, forceDataRefresh, response => {
				viewModel.CreateMore(response, m => viewModel.MoreItems = m, d => viewModel.Items.AddRange(d));
                viewModel.Items.Reset(response.Data);
            }));
        }
    }
}

