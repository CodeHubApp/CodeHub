using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeHub.Core.Data;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.Services
{
    public interface ILoginService
    {
        Task LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain);

        Task LoginWithToken(string apiDomain, string webDomain, string token, bool enterprise);

        Task LoginWithBasic(string domain, string user, string pass, string twoFactor = null);
    }

    public class LoginService : ILoginService
    {
        private static readonly string[] Scopes = { "user", "repo", "notifications", "gist" };

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

        public async Task LoginWithToken(string apiDomain, string webDomain, string token, bool enterprise)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is invalid");
            if (apiDomain != null && !Uri.IsWellFormedUriString(apiDomain, UriKind.Absolute))
                throw new ArgumentException("API Domain is invalid");
            if (webDomain != null && !Uri.IsWellFormedUriString(webDomain, UriKind.Absolute))
                throw new ArgumentException("Web Domain is invalid");

            var credentials = new Octokit.Credentials(token);
            var client = OctokitClientFactory.Create(new Uri(apiDomain), credentials);
            var userInfo = await client.User.Current();

            var scopes = await GetScopes(apiDomain, userInfo.Login, token);
            CheckScopes(scopes);

            var account = (await _accountsService.Get(apiDomain, userInfo.Login)) ?? new Account();
            account.Username = userInfo.Login;
            account.Domain = apiDomain;
            account.WebDomain = webDomain;
            account.IsEnterprise = enterprise;
            account.OAuth = token;
            account.AvatarUrl = userInfo.AvatarUrl;

            await _accountsService.Save(account);
            await _applicationService.LoginAccount(account);
        }

        public async Task LoginWithBasic(string domain, string user, string pass, string twoFactor = null)
        {
            if (string.IsNullOrEmpty(user))
                throw new ArgumentException("Username is invalid");
            if (string.IsNullOrEmpty(pass))
                throw new ArgumentException("Password is invalid");
            if (domain == null || !Uri.IsWellFormedUriString(domain, UriKind.Absolute))
                throw new ArgumentException("Domain is invalid");

            var newAuthorization = new Octokit.NewAuthorization(
                $"CodeHub: {user}", Scopes, Guid.NewGuid().ToString());

            var credentials = new Octokit.Credentials(user, pass);
            var client = OctokitClientFactory.Create(new Uri(domain), credentials);

            var authorization = await (twoFactor == null
                                ? client.Authorization.Create(newAuthorization)
                                : client.Authorization.Create(newAuthorization, twoFactor));

            var existingAccount = await _accountsService.Get(domain, user);
            var account = existingAccount ?? new Account
            {
                Username = user,
                IsEnterprise = true,
                WebDomain = domain,
                Domain = domain
            };

            account.OAuth = authorization.Token;

            await _applicationService.LoginAccount(account);
        }

        private static async Task<List<string>> GetScopes(string domain, string username, string token)
        {
            var client = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, token)));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
            domain = (domain.EndsWith("/", StringComparison.Ordinal) ? domain : domain + "/") + "user";
            var response = await client.GetAsync(domain);

            if (!response.Headers.TryGetValues("X-OAuth-Scopes", out IEnumerable<string> scopes))
                return new List<string>();

            var values = scopes.FirstOrDefault() ?? string.Empty;
            return values.Split(',').Select(x => x.Trim()).ToList();
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
