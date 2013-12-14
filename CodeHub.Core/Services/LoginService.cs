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

		public async Task<LoginData> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain, GitHubAccount account)
        {
			var token = await Task.Run(() => Client.RequestAccessToken(clientId, clientSecret, code, redirect, requestDomain));
			var client = Client.BasicOAuth(token.AccessToken, apiDomain);
			var info = await client.ExecuteAsync(client.AuthenticatedUser.GetInfo());
            var username = info.Data.Login;

            //Does this user exist?
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
			return new LoginData { Client = client, Account = account };
        }

		public async Task<Client> LoginAccount(GitHubAccount account)
        {
            //Create the client
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
            _accounts.Update(account);
            return client;
        }

		public LoginData Authenticate(string domain, string user, string pass, string twoFactor, bool enterprise, GitHubAccount account)
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
                bool exists = account != null;
                if (!exists)
                    account = new GitHubAccount { Username = user };

                account.Domain = apiUrl;
				account.IsEnterprise = enterprise;
                var client = twoFactor == null ? Client.Basic(user, pass, apiUrl) : Client.BasicTwoFactorAuthentication(user, pass, twoFactor, apiUrl);

				if (enterprise)
				{
					account.Password = pass;
				}
				else
				{
	                var auth = client.Execute(client.Authorizations.GetOrCreate("72f4fb74bdba774b759d", "9253ab615f8c00738fff5d1c665ca81e581875cb", new System.Collections.Generic.List<string>(Scopes), "CodeHub", null));
	                account.OAuth = auth.Data.Token;
				}

                if (exists)
                    _accounts.Update(account);
                else
                    _accounts.Insert(account);

				return new LoginData { Client = client, Account = account };
            }
            catch (StatusCodeException ex)
            {
                //Looks like we need to ask for the key!
                if (ex.Headers.ContainsKey("X-GitHub-OTP"))
                    throw new TwoFactorRequiredException();
                throw new Exception("Unable to login as user " + user + ". Please check your credentials and try again.");
            }
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
