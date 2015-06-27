using System;

namespace CodeHub.Core
{
    public static class Secrets
    {
        public static string GithubOAuthId
        {
            get { throw new NotImplementedException("You need to specify the OAuth Client Id"); }
        }

        public static string GithubOAuthSecret
        {
            get { throw new NotImplementedException("You need to specify the OAuth Client Secret"); }
        }
    }
}

