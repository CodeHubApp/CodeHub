using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.Core.Data;
using CodeHub.Core.Utilities;
using GitHubSharp;

namespace CodeHub.Core.Services
{
    public interface ILoginService
    {
        Task LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain);

        Task LoginWithBasic(string domain, string user, string pass, string twoFactor = null);
    }

    public class LoginService : ILoginService
    {
        private static readonly string[] Scopes = { "user", "public_repo", "repo", "notifications", "gist" };

        private readonly IApplicationService _applicationService;
        private readonly IAccountsService _accountsService;

        public LoginService(
            IAccountsService accountsService,
            IApplicationService applicationService)
        {
            _accountsService = accountsService;
            _applicationService = applicationService;
        }

        public async Task LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain)
        {
            var oauthRequest = new Octokit.OauthTokenRequest(clientId, clientSecret, code)
            {
                RedirectUri = new Uri(redirect)
            };

            var client = new Octokit.GitHubClient(OctokitClientFactory.UserAgent);
            var token = await client.Oauth.CreateAccessToken(oauthRequest);

            var credentials = new Octokit.Credentials(token.AccessToken);
            client = OctokitClientFactory.Create(new Uri(apiDomain), credentials);

            var user = await client.User.Current();
            var account = await _accountsService.Get(apiDomain, user.Login);

            account = account ?? new Account { Username = user.Login };
            account.OAuth = token.AccessToken;
            account.AvatarUrl = user.AvatarUrl;
            account.Domain = apiDomain;
            account.WebDomain = requestDomain;

            await _accountsService.Save(account);
            await _applicationService.LoginAccount(account);
        }

        public async Task LoginWithBasic(string domain, string user, string pass, string twoFactor = null)
        {
            var apiUrl = domain;

            //Make some valid checks
            if (string.IsNullOrEmpty(user))
                throw new ArgumentException("Username is invalid");
            if (string.IsNullOrEmpty(pass))
                throw new ArgumentException("Password is invalid");
            if (apiUrl != null && !Uri.IsWellFormedUriString(apiUrl, UriKind.Absolute))
                throw new ArgumentException("Domain is invalid");


            var client = twoFactor == null ? Client.Basic(user, pass, apiUrl) : Client.BasicTwoFactorAuthentication(user, pass, twoFactor, apiUrl);
            var authorization = await client.ExecuteAsync(client.Authorizations.Create(new List<string>(Scopes), "CodeHub: " + user, null, Guid.NewGuid().ToString()));

            var existingAccount = await _accountsService.Get(apiUrl, user);
            var account = existingAccount ?? new Account
            {
                Username = user,
                IsEnterprise = true,
                WebDomain = apiUrl,
                Domain = apiUrl
            };

            account.OAuth = authorization.Data.Token;

            await _applicationService.LoginAccount(account);
        }
    }
}
