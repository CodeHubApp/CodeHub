using GitHubSharp.Models;
using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class IssueEditMessage : MvxMessage
    {
        public IssueEditMessage(object sender) : base(sender) {}
        public IssueModel Issue;
    }
}

