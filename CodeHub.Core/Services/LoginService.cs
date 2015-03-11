using System;
using CodeHub.Core.Data;
using GitHubSharp;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Octokit.Internal;
using Octokit;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.Services
{
    public class LoginService : ILoginService
    {
        private static readonly string[] Scopes = { "user", "repo", "gist", "read:org" };
        private readonly IAccountsRepository _accounts;

        public LoginService(IAccountsRepository accounts)
        {
            _accounts = accounts;
        }

		public async Task<LoginData> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain, GitHubAccount account)
        {
			var token = await Client.RequestAccessToken(clientId, clientSecret, code, redirect, requestDomain);
			var client = Client.BasicOAuth(token.AccessToken, apiDomain);
            var info = (await client.ExecuteAsync(client.AuthenticatedUser.GetInfo())).Data;
            var username = info.Login;

            //Does this user exist?
            var exists = account != null;
			if (!exists)
                account = new GitHubAccount { Username = username };
			account.OAuth = token.AccessToken;
            account.AvatarUrl = info.AvatarUrl;
            account.Name = info.Name;
            account.Email = info.Email;
			account.Domain = apiDomain;
			account.WebDomain = requestDomain;
			client.Username = username;

            if (exists)
                await _accounts.Update(account);
            else
                await _accounts.Insert(account);
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
            account.Name = userInfo.Name;
            account.Email = userInfo.Email;
            account.AvatarUrl = userInfo.AvatarUrl;
			client.Username = userInfo.Login;
            await _accounts.Update(account);
            return client;
        }

        public async Task<GitHubAccount> Authenticate(string apiDomain, string webDomain, string user, string pass, string twoFactor, bool enterprise)
        {
            try
            {
//                var client = twoFactor == null ? Client.Basic(user, pass, apiDomain) : Client.BasicTwoFactorAuthentication(user, pass, twoFactor, apiDomain);
//                var auth = await client.ExecuteAsync(client.Authorizations.GetOrCreate("72f4fb74bdba774b759d", "9253ab615f8c00738fff5d1c665ca81e581875cb", Scopes.ToList(), "CodeHub", null));

                var client = OctokitClientFactory.Create(new Uri(apiDomain), new Credentials(user, pass));
                var appAuth = await client.Authorization.GetOrCreateApplicationAuthentication("72f4fb74bdba774b759d", 
                    "9253ab615f8c00738fff5d1c665ca81e581875cb", new NewAuthorization("CodeHub", Scopes));

                if (Scopes.Except(appAuth.Scopes).Any())
                {
                    await client.Authorization.Update(appAuth.Id, new AuthorizationUpdate { Scopes = Scopes });
                }

                return await Authenticate(apiDomain, webDomain, appAuth.Token, enterprise);
            }
            catch (StatusCodeException ex)
            {
                //Looks like we need to ask for the key!
                if (ex.Headers.ContainsKey("X-GitHub-OTP"))
                    throw new TwoFactorRequiredException();
                throw new Exception("Unable to login as user " + user + ". Please check your credentials and try again.");
            }
        }

        public async Task<GitHubAccount> Authenticate(string apiDomain, string webDomain, string token, bool enterprise)
        {
            //Make some valid checks
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is invalid");
            if (apiDomain != null && !Uri.IsWellFormedUriString(apiDomain, UriKind.Absolute))
                throw new ArgumentException("API Domain is invalid");
            if (webDomain != null && !Uri.IsWellFormedUriString(webDomain, UriKind.Absolute))
                throw new ArgumentException("Web Domain is invalid");

            var client = Client.BasicOAuth(token, apiDomain);
            var userInfo = (await client.ExecuteAsync(client.AuthenticatedUser.GetInfo())).Data;

            var scopes = await GetScopes(apiDomain, token);
            CheckScopes(scopes);

            GitHubAccount account;
            try { account = await _accounts.Find(apiDomain, userInfo.Login); }
            catch { account = new GitHubAccount(); }

            account.Username = userInfo.Login;
            account.Domain = apiDomain;
            account.WebDomain = webDomain;
            account.IsEnterprise = enterprise;
            account.OAuth = token;
            account.Username = userInfo.Login;
            account.Name = userInfo.Name;
            account.Email = userInfo.Email;
            account.AvatarUrl = userInfo.AvatarUrl;

            await _accounts.Insert(account);
            return account;
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

        private static async Task<List<string>> GetScopes(string domain, string token)
        {
            var client = new HttpClient(new ModernHttpClient.NativeMessageHandler());
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", token, "x-oauth-basic")));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
            domain = (domain.EndsWith("/", StringComparison.Ordinal) ? domain : domain + "/") + "user";
            var response = await client.GetAsync(domain);
            var values = response.Headers.GetValues("X-OAuth-Scopes").FirstOrDefault();
            return values == null ? new List<string>() : values.Split(',').Select(x => x.Trim()).ToList();
        }

        private static void CheckScopes(IEnumerable<string> scopes)
        {
            var missing = Scopes.Except(scopes).ToList();
            if (missing.Any())
                throw new InvalidOperationException("Missing scopes! You are missing access to the following " + 
                    "scopes that are necessary for CodeHub to operate correctly: " + string.Join(", ", missing));
        }
    }
}
