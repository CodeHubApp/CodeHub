using System;
using System.Threading.Tasks;

namespace CodeHub.ViewModels
{
    public static class ViewModelExtensions
    {
        public static void RequestModel<TRequest>(this CodeFramework.ViewModels.ViewModelBase viewModel, GitHubSharp.GitHubRequest<TRequest> request, bool forceDataRefresh, System.Action<GitHubSharp.GitHubResponse<TRequest>> update) where TRequest : new()
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
                System.Threading.Tasks.Task.Run(() => {
                    try
                    {
                        request.RequestFromCache = false;
                        update(Application.Client.Execute(request));
                    }
                    catch (GitHubSharp.NotModifiedException)
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

        public static void CreateMore<T>(this CodeFramework.ViewModels.ViewModelBase viewModel, 
                                         GitHubSharp.GitHubResponse<T> response, 
                                         Action<Task> assignMore, 
                                         Action<T> newDataAction) where T : new()
        {
            if (response.More == null)
            {
                assignMore(null);
                return;
            }

            assignMore(new Task(async () => {
                response.More.UseCache = false;
                var moreResponse = await Application.Client.ExecuteAsync(response.More);
                viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                newDataAction(moreResponse.Data);
            }));
        }
    }
}

