using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class SourceEditMessage
    {
        public string OldSha;
        public string Data;
        public ContentUpdateModel Update;
    }
}

