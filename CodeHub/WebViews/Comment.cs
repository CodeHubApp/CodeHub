using System;

namespace CodeHub.WebViews
{
    public class Comment
    {
        public string AvatarUrl { get; private set; }

        public string Name { get; private set; }

        public string Date { get; private set; }

        public string Body { get; private set; }

        public Comment(Uri avatar, string name, string body, string date)
        {
            if (avatar != null)
                AvatarUrl = avatar.AbsoluteUri;
            Name = name;
            Body = body;
            Date = date;
        }
    }
}

