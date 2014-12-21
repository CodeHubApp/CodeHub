using System;
using CodeHub.Core.Services;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Xamarin.Utilities.XCallback;

namespace CodeHub.iOS
{
    public class XCallbackProvider
    {
        private readonly IApplicationService _applicationService;

        public XCallbackProvider(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        public bool Handle(XCallbackQuery query)
        {
            if (query.Url == "/gist/create")
            {
                var description = query.Parameters.ContainsKey("description") ? query.Parameters["description"] : null;
                var isPublic = query.Parameters.ContainsKey("public") && bool.Parse(query.Parameters["public"]);
                var files = new Dictionary<string, GitHubSharp.Models.GistCreateModel.File>();

                var fileCounter = 0;
                foreach (var param in query.Parameters)
                {
                    if (param.Key.StartsWith("file", StringComparison.Ordinal))
                        files.Add("gistfile" + (++fileCounter) + ".txt", new GitHubSharp.Models.GistCreateModel.File { Content = param.Value });
                }

//                var vm = IoC.Resolve<Core.ViewModels.Gists.GistCreateViewModel>();
//                vm.Files
//
//                txService.Add(new GitHubSharp.Models.GistCreateModel { Description = description, Public = isPublic, Files = files });
//                SetupErrorCancelCallbacks<CodeHub.Core.ViewModels.Gists.GistCreateViewModel>(query);
//                SetupSuccessCallback<GistAddMessage, CodeHub.Core.ViewModels.Gists.GistCreateViewModel>(query, (msg) =>
//                {
//                    return new Dictionary<string, string> { { "id", msg.Gist.Id } };
//                });
//
//                var rec = new MvxViewModelRequest();
//                rec.ViewModelType = typeof(CodeHub.Core.ViewModels.Gists.GistCreateViewModel);
//                appService.SetUserActivationAction(() => viewDispatcher.ShowViewModel(rec));

                //UIApplication.SharedApplication.OpenUrl(new NSUrl(query.ExpandSuccessUrl(callback(msg))));

                return true;
            }

            return false;
        }
    }
}

