using System;
using CodeFramework.iOS.XCallback;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.Services;
using System.Collections.Generic;
using Cirrious.MvvmCross.Plugins.Messenger;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using CodeHub.Core.Messages;

namespace CodeHub.iOS
{
    public static class XCallbackProvider
    {
        private static MvxSubscriptionToken _successToken, _errorToken, _cancelToken;

        public static bool Handle(XCallbackQuery query)
        {
            var viewDispatcher = Mvx.Resolve<Cirrious.MvvmCross.Views.IMvxViewDispatcher>();
            var txService = Mvx.Resolve<CodeFramework.Core.Services.IViewModelTxService>();
            var appService = Mvx.Resolve<IApplicationService>();

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

                txService.Add(new GitHubSharp.Models.GistCreateModel { Description = description, Public = isPublic, Files = files });
                SetupErrorCancelCallbacks<CodeHub.Core.ViewModels.Gists.GistCreateViewModel>(query);
                SetupSuccessCallback<GistAddMessage, CodeHub.Core.ViewModels.Gists.GistCreateViewModel>(query, (msg) =>
                {
                    return new Dictionary<string, string> { { "id", msg.Gist.Id } };
                });

                var rec = new MvxViewModelRequest();
                rec.ViewModelType = typeof(CodeHub.Core.ViewModels.Gists.GistCreateViewModel);
                appService.SetUserActivationAction(() => viewDispatcher.ShowViewModel(rec));
                return true;
            }

            return false;
        }

        public static void DestoryTokens()
        {
            if (_cancelToken != null)
                _cancelToken.Dispose();
            _cancelToken = null;

            if (_errorToken != null)
                _errorToken.Dispose();
            _errorToken = null;

            if (_successToken != null)
                _successToken.Dispose();
            _successToken = null;
        }

        private static void SetupSuccessCallback<TMessage, TViewModel>(XCallbackQuery query, Func<TMessage, IDictionary<string, string>> callback) where TMessage : MvxMessage
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            if (!string.IsNullOrEmpty(query.SuccessUrl))
            {
                _successToken = messenger.Subscribe<TMessage>(msg => 
                {
                    if (!(msg.Sender is TViewModel))
                        return;

                    DestoryTokens();
                    UIApplication.SharedApplication.OpenUrl(new NSUrl(query.ExpandSuccessUrl(callback(msg))));
                });
            }
        }

        private static void SetupErrorCancelCallbacks<T>(XCallbackQuery query)
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            if (!string.IsNullOrEmpty(query.CancelUrl))
            {
                _cancelToken = messenger.Subscribe<CodeFramework.Core.Messages.CancelationMessage>(msg => 
                {
                    if (!(msg.Sender is T))
                        return;

                    DestoryTokens();
                    UIApplication.SharedApplication.OpenUrl(new NSUrl(query.CancelUrl));
                });
            }

            if (!string.IsNullOrEmpty(query.ErrorUrl))
            {
                _cancelToken = messenger.Subscribe<CodeFramework.Core.Messages.ErrorMessage>(msg => 
                {
                    if (!(msg.Sender is T))
                        return;

                    DestoryTokens();
                    UIApplication.SharedApplication.OpenUrl(new NSUrl(query.ExpandErrorUrl(-1, msg.Error.Message)));
                });
            }
        }
    }
}

