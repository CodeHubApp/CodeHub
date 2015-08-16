using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;
using Splat;
using System.Reactive.Linq;
using System.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels
{
    public static class ViewModelExtensions
    {
        public static T CreateViewModel<T>(this IBaseViewModel @this)
        {
            return Locator.Current.GetService<IServiceConstructor>().Construct<T>();
        }

        public static Task ShowAlertDialog(this IBaseViewModel @this, string title, string message)
        {
            return Locator.Current.GetService<IAlertDialogFactory>().Alert(title, message);
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
                var moreResponse = await Locator.Current.GetService<ISessionService>().Client.ExecuteAsync(response.More);
                viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                newDataAction(moreResponse.Data);
            });
        }

        public static async Task SimpleCollectionLoad<T>(this IReactiveList<T> viewModel, GitHubRequest<List<T>> request, Action<Func<Task>> assignMore = null) where T : new()
        {
            if (assignMore == null)
                assignMore = (x) => {};

            var application = Locator.Current.GetService<ISessionService>();
            var response = await application.Client.ExecuteAsync(request);

            viewModel.CreateMore(response, assignMore, x => viewModel.AddRange(x));
            viewModel.Reset(response.Data);
        }
    }
}

