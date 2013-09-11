using System;
using CodeFramework.Controllers;
using System.Collections.Generic;
using CodeFramework.Filters.Models;

namespace CodeHub.Controllers
{
    public static class ListControllerExtensions
    {
        public static Action CreateMore<T>(this ListController<T> controller, GitHubSharp.GitHubResponse<List<T>> response, Func<List<T>, List<T>> cleanDelegate = null, Action callback = null) where T : new()
        {
            if (response.More == null)
                return null;

            return () => {
                var data = response.More();
                var items = data.Data;
                if (cleanDelegate != null)
                    items = cleanDelegate(items);
                controller.Model.Data.AddRange(items);
                controller.Model.More = controller.CreateMore(data);
                controller.Render();

                if (callback != null)
                    callback();
            };
        }

        public static Action CreateMore<T, F>(this ListController<T, F> controller, GitHubSharp.GitHubResponse<List<T>> response, Func<List<T>, List<T>> cleanDelegate = null, Action callback = null) where T : new() where F : FilterModel<F>, new()
        {
            if (response.More == null)
                return null;

            return () => {
                var data = response.More();
                var items = data.Data;
                if (cleanDelegate != null)
                    items = cleanDelegate(items);
                controller.Model.Data.AddRange(items);
                controller.Model.More = controller.CreateMore(data);
                controller.Render();

                if (callback != null)
                    callback();
            };
        }
    }
}

