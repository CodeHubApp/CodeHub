namespace CodeHub.Core.Messages
{
    public class GistAddMessage
    {
        public Octokit.Gist Gist { get; private set; }

        public GistAddMessage(Octokit.Gist gist)
        {
            Gist = gist;
        }
    }
}

