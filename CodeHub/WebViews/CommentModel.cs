using System.Collections.Generic;

namespace CodeHub.WebViews
{
    public class CommentModel
    {
        public IList<Comment> Comments { get; private set; }

        public int FontSize { get; private set; }

        public CommentModel(IList<Comment> comments, int fontSize)
        {
            Comments = comments;
            FontSize = fontSize;
        }
    }
}

