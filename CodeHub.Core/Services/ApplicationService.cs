using CodeHub.Core.Data;
using GitHubSharp;
using System;
using System.Threading.Tasks;
using CodeHub.Core.Utilities;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private static readonly string[] Scopes = { "user", "public_repo", "repo", "notifications", "gist" };
        private readonly IAccountsService _accountsService;

        public Client Client { get; private set; }

        public Octokit.GitHubClient GitHubClient { get; private set; }

        public Account Account { get; private set; }

        public Action ActivationAction { get; set; }

        public ApplicationService(IAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        public void DeactivateUser()
        {
            _accountsService.SetActiveAccount(null).Wait();
            Account = null;
            Client = null;
        }

        public void ActivateUser(Account account, Client client)
        {
            _accountsService.SetActiveAccount(account).Wait();
            Account = account;
            Client = client;

            var domain = account.Domain ?? Client.DefaultApi;
            var credentials = new Octokit.Credentials(account.OAuth);
            var oldClient = Client.BasicOAuth(account.OAuth, domain);
            GitHubClient = OctokitClientFactory.Create(new Uri(domain), credentials);
        }

        public void SetUserActivationAction(Action action)
        {
            if (Account != null)
                action();
            else
                ActivationAction = action;
        }

        public Task UpdateActiveAccount() => _accountsService.Save(Account);

        public async Task<Tuple<Client, Account>> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain)
        {
            var token = await Client.RequestAccessToken(clientId, clientSecret, code, redirect, requestDomain);
            var client = Client.BasicOAuth(token.AccessToken, apiDomain);
            var info = await client.ExecuteAsync(client.AuthenticatedUser.GetInfo());
            var username = info.Data.Login;

            var account = await _accountsService.Get(apiDomain, username);
            var exists = account != null;
            account = account ?? new Account { Username = username };
            account.OAuth = token.AccessToken;
            account.AvatarUrl = info.Data.AvatarUrl;
            account.Domain = apiDomain;
            account.WebDomain = requestDomain;
            client.Username = username;
            await _accountsService.Save(account);
            return new Tuple<Client, Account>(client, account);
        }

        public async Task<Client> LoginAccount(Account account)
        {
            Client client = null;
            if (!string.IsNullOrEmpty(account.OAuth))
            {
                client = Client.BasicOAuth(account.OAuth, account.Domain ?? Client.DefaultApi);
            }
            else if (account.IsEnterprise || !string.IsNullOrEmpty(account.Password))
            {
                client = Client.Basic(account.Username, account.Password, account.Domain ?? Client.DefaultApi);
            }

            var data = await client.ExecuteAsync(client.AuthenticatedUser.GetInfo());
            var userInfo = data.Data;
            account.Username = userInfo.Login;
            account.AvatarUrl = userInfo.AvatarUrl;
            client.Username = userInfo.Login;
            await _accountsService.Save(account);
            return client;
        }

        public async Task<Account> LoginWithBasic(string domain, string user, string pass, string twoFactor = null)
        {
            //Fill these variables in during the proceeding try/catch
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
            return account;
        }
    }
}
