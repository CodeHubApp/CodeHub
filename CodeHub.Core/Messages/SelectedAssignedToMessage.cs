using GitHubSharp.Models;
using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class SelectedAssignedToMessage : MvxMessage
    {
        public SelectedAssignedToMessage(object sender) : base(sender) {}
        public BasicUserModel User;
    }
}

