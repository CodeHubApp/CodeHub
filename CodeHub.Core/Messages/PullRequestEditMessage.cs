using Cirrious.MvvmCross.Plugins.Messenger;
using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class PullRequestEditMessage : MvxMessage
    {
        public PullRequestEditMessage(object sender) : base(sender) {}
        public PullRequestModel PullRequest;
    }
}

