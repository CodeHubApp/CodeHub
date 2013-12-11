using System;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;
using GitHubSharp;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public class LoginService : ILoginService
    {
        private static readonly string[] Scopes = { "user", "public_repo", "repo", "notifications", "gist" };
        private readonly IAccountsService _accounts;

        public LoginService(IAccountsService accounts)
        {
            _accounts = accounts;
        }

		public async Task<Client> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain)
        {
			var token = await Task.Run(() => Client.RequestAccessToken(clientId, clientSecret, code, redirect, requestDomain));
			var client = Client.BasicOAuth(token.AccessToken, apiDomain);
			var info = await client.ExecuteAsync(client.AuthenticatedUser.GetInfo());
            var username = info.Data.Login;

            //Does this user exist?
            var account = (GitHubAccount)_accounts.Find(username);
            var exists = account != null;
            if (!exists)
                account = new GitHubAccount { Username = username };
			account.OAuth = token.AccessToken;
            account.AvatarUrl = info.Data.AvatarUrl;
			account.Domain = apiDomain;
			account.WebDomain = requestDomain;
			client.Username = username;

            if (exists)
                _accounts.Update(account);
            else
                _accounts.Insert(account);
            return client;
        }

		public async Task<Client> LoginAccount(GitHubAccount account)
        {
            //Create the client
            var client = Client.BasicOAuth(account.OAuth, account.Domain ?? Client.DefaultApi);
			var data = await client.ExecuteAsync(client.AuthenticatedUser.GetInfo());
			var userInfo = data.Data;
            account.Username = userInfo.Login;
            account.AvatarUrl = userInfo.AvatarUrl;
			client.Username = userInfo.Login;
            _accounts.Update(account);
            return client;
        }

        public GitHubAccount Authenticate(string domain, string user, string pass, string twoFactor)
        {
            //Fill these variables in during the proceeding try/catch
            var apiUrl = domain;

            try
            {
                //Make some valid checks
                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("Username is invalid");
                if (string.IsNullOrEmpty(pass))
                    throw new ArgumentException("Password is invalid");
                if (apiUrl != null && !Uri.IsWellFormedUriString(apiUrl, UriKind.Absolute))
                    throw new ArgumentException("Domain is invalid");

                //Does this user exist?
                var account = (GitHubAccount)_accounts.Find(user);
                bool exists = account != null;
                if (!exists)
                    account = new GitHubAccount { Username = user };

                account.Domain = apiUrl;
                var client = twoFactor == null ? Client.Basic(user, pass, apiUrl) : Client.BasicTwoFactorAuthentication(user, pass, twoFactor, apiUrl);
                var auth = client.Execute(client.Authorizations.GetOrCreate("72f4fb74bdba774b759d", "9253ab615f8c00738fff5d1c665ca81e581875cb", new System.Collections.Generic.List<string>(Scopes), "CodeHub", null));
                account.OAuth = auth.Data.Token;

                if (exists)
                    _accounts.Update(account);
                else
                    _accounts.Insert(account);

                return account;
            }
            catch (StatusCodeException ex)
            {
                //Looks like we need to ask for the key!
                if (ex.Headers.ContainsKey("X-GitHub-OTP"))
                    throw new TwoFactorRequiredException();
                throw new Exception("Unable to login as user " + user + ". Please check your credentials and try again.");
            }
        }

        public bool NeedsAuthentication(GitHubAccount account)
        {
            var exists = _accounts.Find(account.Username) != null;
            return exists == false || string.IsNullOrEmpty(account.OAuth);
        }

        /// <summary>
        /// Goofy exception so we can catch and do two factor auth
        /// </summary>
        public class TwoFactorRequiredException : Exception
        {
            public TwoFactorRequiredException()
                : base("Two Factor Authentication is Required!")
            {
            }
        }
    }
}
