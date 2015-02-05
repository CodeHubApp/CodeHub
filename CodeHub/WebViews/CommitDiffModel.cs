using System;

namespace CodeHub.WebViews
{
    public class CommitDiffModel
    {
        public string[] Lines { get; private set; }

        public int FontSize { get; private set; }

        public CommitDiffModel(string[] lines, int fontSize)
        {
            Lines = lines;
            FontSize = fontSize;
        }
    }
}

