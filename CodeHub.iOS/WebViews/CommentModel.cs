using System.Collections.Generic;
using System;
using System.Linq;

namespace CodeHub.iOS.WebViews
{
    public class Comment
    {
        public string AvatarUrl { get; private set; }

        public string Name { get; private set; }

        public DateTimeOffset Date { get; private set; }

        public string Body { get; private set; }

        public Comment(string avatar, string name, string body, DateTimeOffset date)
        {
            AvatarUrl = avatar;
            Name = name;
            Body = body;
            Date = date;
        }
    }

    public class CommentModel
    {
        public IList<Comment> Comments { get; private set; }

        public int FontSize { get; private set; }

        public CommentModel(IEnumerable<Comment> comments, int fontSize)
        {
            Comments = (comments ?? Enumerable.Empty<Comment>()).ToList();
            FontSize = fontSize;
        }
    }
}

