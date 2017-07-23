namespace CodeHub.WebViews
{
    public class MarkdownModel
    {
        public string Body { get; }

        public int FontSize { get; }

        public bool ContinuousResize { get; }

        public MarkdownModel(string body, int fontSize, bool continuousResize = false)
        {
            Body = body;
            FontSize = fontSize;
            ContinuousResize = continuousResize;
        }
    }
}

