using CodeHub.Core.Data;

namespace CodeHub.Core.Messages
{
    public class LoggingInMessage
    {
        public GitHubAccount Account { get; private set; }
        
        public LoggingInMessage(GitHubAccount account)
        {
            Account = account;
        }
    }
}

