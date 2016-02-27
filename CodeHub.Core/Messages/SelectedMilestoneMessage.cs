using GitHubSharp.Models;
using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class SelectedMilestoneMessage : MvxMessage
    {
        public SelectedMilestoneMessage(object sender) : base(sender) {}
        public MilestoneModel Milestone;
    }
}

