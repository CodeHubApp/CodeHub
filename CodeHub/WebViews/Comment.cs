namespace CodeHub.WebViews
{
    public class Comment
    {
        public string AvatarUrl { get; private set; }

        public string Name { get; private set; }

        public string Date { get; private set; }

        public string Body { get; private set; }

        public Comment(string avatarUrl, string name, string body, string date)
        {
            AvatarUrl = avatarUrl;
            Name = name;
            Body = body;
            Date = date;
        }
    }
}

