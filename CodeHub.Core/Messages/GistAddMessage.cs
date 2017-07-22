namespace CodeHub.Core.Messages
{
    public class GistAddMessage
    {
        public Octokit.Gist Gist { get; }

        public GistAddMessage(Octokit.Gist gist)
        {
            Gist = gist;
        }
    }
}

