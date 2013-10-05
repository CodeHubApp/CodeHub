using System;
using CodeFramework.Controllers;
using System.Collections.Generic;
using CodeFramework.Filters.Models;

namespace CodeHub.Controllers
{
    public static class ControllerExtensions
    {
        public static Action CreateMore<T>(this ListController<T> controller, GitHubSharp.GitHubResponse<List<T>> response, Func<List<T>, List<T>> cleanDelegate = null, Action callback = null) where T : new()
        {
            if (response.More == null)
                return null;

            return () => {
                response.More.UseCache = false;
                var data = Application.Client.Execute(response.More);
                var items = data.Data;
                if (cleanDelegate != null)
                    items = cleanDelegate(items);
                controller.Model.Data.AddRange(items);
                controller.Model.More = controller.CreateMore(data);
                controller.Refresh();

                if (callback != null)
                    callback();
            };
        }

        public static Action CreateMore<T, F>(this ListController<T, F> controller, GitHubSharp.GitHubResponse<List<T>> response, Func<List<T>, List<T>> cleanDelegate = null, Action callback = null) where T : new() where F : FilterModel<F>, new()
        {
            if (response.More == null)
                return null;

            return () => {
                response.More.UseCache = false;
                var data = Application.Client.Execute(response.More);
                var items = data.Data;
                if (cleanDelegate != null)
                    items = cleanDelegate(items);
                controller.Model.Data.AddRange(items);
                controller.Model.More = controller.CreateMore(data);
                controller.Refresh();

                if (callback != null)
                    callback();
            };
        }

        public static void RequestModel<TController, TRequest>(this Controller<TController> controller, GitHubSharp.GitHubRequest<TRequest> request, bool forceDataRefresh, System.Action<GitHubSharp.GitHubResponse<TRequest>> update) where TRequest : new() where TController : class, new()
        {
            try
            {
                var stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                if (forceDataRefresh)
                {
                    request.CheckIfModified = false;
                    request.RequestFromCache = false;
                }

                var response = Application.Client.Execute(request);
                stopWatch.Stop();

                Console.WriteLine("Request executed in: " + stopWatch.ElapsedMilliseconds + "ms");

                stopWatch.Reset();
                stopWatch.Start();
                update(response);
                stopWatch.Stop();

                Console.WriteLine("View updated in: " + stopWatch.ElapsedMilliseconds + "ms");


                if (response.WasCached)
                {
                    controller.View.ShowLoading(true, () => {
                        try
                        {
                            request.RequestFromCache = false;
                            update(Application.Client.Execute(request));
                        }
                        catch (GitHubSharp.NotModifiedException)
                        {
                        }
                    });
                }
            }
            catch (Exception e)
            {
                MonoTouch.Utilities.LogException("Unable to request model", e);
            }
        }
    }
}

