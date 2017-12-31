using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class SelectedAssignedToMessage
    {
        public BasicUserModel User { get; }

        public SelectedAssignedToMessage(BasicUserModel user)
        {
            User = user;
        }
    }
}

