using System;
using CodeHub.Core.Data;
using GitHubSharp;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Octokit;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.Services
{
    public class LoginService : ILoginService
    {
        private readonly IAccountsRepository _accounts;

        public LoginService(IAccountsRepository accounts)
        {
            _accounts = accounts;
        }

        public async Task<GitHubAccount> Authenticate(string apiDomain, string webDomain, string user, string pass, string twoFactor, bool enterprise)
        {
            try
            {
                string token = null;

                if (enterprise)
                {
                    var client = twoFactor == null ? Client.Basic(user, pass, apiDomain) : Client.BasicTwoFactorAuthentication(user, pass, twoFactor, apiDomain);
                    var name = string.Format("CodeHub (GitHub for iOS) - {0}", DateTime.Now.ToString("s"));
                    var request = client.Authorizations.Create(OctokitClientFactory.Scopes.ToList(), name, "http://codehub-app.com");
                    var auth = await client.ExecuteAsync(request);
                    token = auth.Data.Token;
                }
                else
                {
                    var scopes = OctokitClientFactory.Scopes;
                    var client = OctokitClientFactory.Create(new Uri(apiDomain), new Credentials(user, pass));

                    var appAuth = await client.Authorization.GetOrCreateApplicationAuthentication("72f4fb74bdba774b759d", 
                        "9253ab615f8c00738fff5d1c665ca81e581875cb", new NewAuthorization("CodeHub", scopes));

                    if (scopes.Except(appAuth.Scopes).Any())
                        await client.Authorization.Update(appAuth.Id, new AuthorizationUpdate { Scopes = scopes });

                    token = appAuth.Token;
                }

                return await Authenticate(apiDomain, webDomain, token, enterprise);
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

            var account = (await _accounts.Find(apiDomain, userInfo.Login)) ?? new GitHubAccount();
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
            var missing = OctokitClientFactory.Scopes.Except(scopes).ToList();
            if (missing.Any())
                throw new InvalidOperationException("Missing scopes! You are missing access to the following " + 
                    "scopes that are necessary for CodeHub to operate correctly: " + string.Join(", ", missing));
        }
    }
}
