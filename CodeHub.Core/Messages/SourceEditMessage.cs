using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class SourceEditMessage
    {
	    public SourceEditMessage(string oldSha, string data, ContentUpdateModel update)
	    {
	        Update = update;
	        Data = data;
	        OldSha = oldSha;
	    }

	    public string OldSha { get; private set; }
        public string Data { get; private set; }
        public ContentUpdateModel Update { get; private set; }
    }
}

