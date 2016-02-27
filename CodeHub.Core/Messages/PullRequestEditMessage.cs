using GitHubSharp.Models;
using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class PullRequestEditMessage : MvxMessage
    {
        public PullRequestEditMessage(object sender) : base(sender) {}
        public PullRequestModel PullRequest;
    }
}

