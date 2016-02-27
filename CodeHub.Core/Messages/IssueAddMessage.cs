using GitHubSharp.Models;
using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class IssueAddMessage : MvxMessage
    {
        public IssueAddMessage(object sender) : base(sender) {}
        public IssueModel Issue;
    }
}

