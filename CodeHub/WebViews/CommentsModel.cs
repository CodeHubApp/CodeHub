using System.Collections.Generic;
using System;
using System.Linq;
using Humanizer;

namespace CodeHub.WebViews
{
    public class Comment
    {
        public string AvatarUrl { get; }

        public string Name { get; }

        public DateTimeOffset Date { get; }

        public string DateString => Date.Humanize();

        public string Body { get; }

        public Comment(string avatar, string name, string body, DateTimeOffset date)
        {
            AvatarUrl = avatar;
            Name = name;
            Body = body;
            Date = date;
        }
    }

    public class CommentsModel
    {
        public IList<Comment> Comments { get; }

        public int FontSize { get; }

        public CommentsModel(IEnumerable<Comment> comments, int fontSize)
        {
            Comments = (comments ?? Enumerable.Empty<Comment>()).ToList();
            FontSize = fontSize;
        }
    }
}

