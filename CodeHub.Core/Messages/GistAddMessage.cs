using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class GistAddMessage
    {
        public GistModel Gist { get; private set; }

        public GistAddMessage(GistModel gist) 
        {
            Gist = gist;
        }
    }
}

