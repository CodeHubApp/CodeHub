using System;

namespace CodeHub.Core
{
    public static class Secrets
    {
        public static string GithubOAuthId
        {
            get { throw new InvalidOperationException("You must get your own Key"); }
        }

        public static string GithubOAuthSecret
        {
            get { throw new InvalidOperationException("You must get your own Secret"); }
        }

        public static string ErrorReportingKey
        {
            get { throw new InvalidOperationException("You must have a valid error reporting key"); }
        }
    }
}