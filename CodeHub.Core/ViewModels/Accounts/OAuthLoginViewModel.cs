using System;
using ReactiveUI;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using Splat;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class OAuthLoginViewModel : ReactiveObject
    {
        public static readonly string RedirectUri = "http://dillonbuchanan.com/";
        private readonly IApplicationService _applicationService;
        private readonly IAlertDialogService _alertDialogService;

        public string LoginUrl
        {
            get
            {
                var web = WebDomain.TrimEnd('/');
                return string.Format(
                    web + "/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}",
                    Secrets.GithubOAuthId,
                    Uri.EscapeDataString(OAuthLoginViewModel.RedirectUri),
                    Uri.EscapeDataString("user:follow,repo,notifications,gist,read:org"));
            }
        }

        public string WebDomain { get; set; } = GitHubSharp.Client.AccessTokenUri;

        public ReactiveCommand<string, Unit> LoginCommand { get; }

        public OAuthLoginViewModel(
            IApplicationService applicationService = null,
            IAlertDialogService alertDialogService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _alertDialogService = alertDialogService ?? Locator.Current.GetService<IAlertDialogService>();

            LoginCommand = ReactiveCommand.CreateFromTask<string>(async code =>
            {
                var login = await _applicationService.LoginWithToken(Secrets.GithubOAuthId, Secrets.GithubOAuthSecret,
                    code, RedirectUri, WebDomain, GitHubSharp.Client.DefaultApi);
                _applicationService.ActivateUser(login.Item2, login.Item1);
                MessageBus.Current.SendMessage(new LogoutMessage());
            });

            LoginCommand
                .ThrownExceptions
                .Subscribe(err => _alertDialogService.Alert("Error!", err.Message).ToBackground());
        }
    }
}

