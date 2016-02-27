using GitHubSharp.Models;
using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class SourceEditMessage : MvxMessage
    {
        public SourceEditMessage(object sender) : base(sender) {}

        public string OldSha;
        public string Data;
        public ContentUpdateModel Update;
    }
}

