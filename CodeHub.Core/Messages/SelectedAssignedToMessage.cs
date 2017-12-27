namespace CodeHub.Core.Messages
{
    public class SelectedAssignedToMessage
    {
        public Octokit.User User { get; }

        public SelectedAssignedToMessage(Octokit.User user)
        {
            User = user;
        }
    }
}

