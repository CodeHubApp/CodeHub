using System.Collections.Generic;
using System.Linq;

namespace CodeHub.WebViews
{
    public class CommitDiffModel
    {
        public string[] Lines { get; private set; }

        public int FontSize { get; private set; }

        public IList<CommitCommentModel> Comments { get; private set; }

        public ILookup<int, CommitCommentModel> CommentsLookup { get; private set; }

        public CommitDiffModel(string[] lines, IEnumerable<CommitCommentModel> comments, int fontSize)
        {
            Lines = lines;
            FontSize = fontSize;
            Comments = comments.ToList();
            CommentsLookup = Comments.ToLookup(x => x.Line);
        }
    }
}

