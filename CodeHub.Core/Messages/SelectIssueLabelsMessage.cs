using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class SelectIssueLabelsMessage : MvxMessage
    {
        public SelectIssueLabelsMessage(object sender) : base(sender) {}
        public GitHubSharp.Models.LabelModel[] Labels;
    }
}

