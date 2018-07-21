namespace CodeHub.WebViews
{
    public class MarkdownModel
    {
        public string Body { get; }

        public int FontSize { get; }

        public bool ContinuousResize { get; }

        public string BaseUrl { get; }

        public MarkdownModel(string body, int fontSize, bool continuousResize = false, string baseUrl = null)
        {
            Body = body;
            FontSize = fontSize;
            ContinuousResize = continuousResize;
            BaseUrl = baseUrl;
        }
    }
}

