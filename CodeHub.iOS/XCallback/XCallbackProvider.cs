using System;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using System.Collections.Generic;
using UIKit;
using Foundation;
using System.Reactive.Linq;
using CodeHub.iOS.ViewControllers.Gists;

namespace CodeHub.iOS.XCallback
{
    public static class XCallbackProvider
    {
        public static bool Handle(XCallbackQuery query)
        {
            var appService = Mvx.Resolve<IApplicationService>();

            if (query.Url == "/gist/create")
            {
                var description = query.Parameters.ContainsKey("description") ? query.Parameters["description"] : null;
                var isPublic = query.Parameters.ContainsKey("public") && bool.Parse(query.Parameters["public"]);
                var files = new Dictionary<string, string>();

                var fileCounter = 0;
                foreach (var param in query.Parameters)
                {
                    if (param.Key.StartsWith("file", StringComparison.Ordinal))
                        files.Add("gistfile" + (++fileCounter) + ".txt", param.Value);
                }

                appService.SetUserActivationAction(() => {
                    var app = UIApplication.SharedApplication.Delegate as AppDelegate;
                    var ctrl = app?.Window?.GetVisibleViewController();
                    if (ctrl == null)
                        return;
                    
                    var view = GistCreateViewController.Show(ctrl);
                    view.ViewModel.Description = description;
                    view.ViewModel.Public = isPublic;
                    view.ViewModel.Files = files;

                    view.ViewModel.SaveCommand.Take(1).Subscribe(x => {
                        var msg = new Dictionary<string, string> { { "id", x.Id } };
                        UIApplication.SharedApplication.OpenUrl(new NSUrl(query.ExpandSuccessUrl(msg)));
                    });

                    view.ViewModel.CancelCommand.Take(1).Subscribe(_ => {
                        UIApplication.SharedApplication.OpenUrl(new NSUrl(query.CancelUrl));
                    });
                });

                return true;
            }

            return false;
        }
    }
}

