namespace CodeHub.Core.ViewModels
{
    public class FileDiffCommentViewModel
    {
        public string Name { get; private set; }

        public string AvatarUrl { get; private set; }

        public string Body { get; private set; }

        public int Line { get; private set; }

        public FileDiffCommentViewModel(string name, string avatarUrl, string body, int line)
        {
            this.Name = name;
            this.AvatarUrl = avatarUrl;
            this.Body = body;
            this.Line = line;
        }
    }
}

