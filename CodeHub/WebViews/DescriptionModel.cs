using System;

namespace CodeHub.WebViews
{
    public class DescriptionModel
    {
        public string Body { get; private set; }

        public int FontSize { get; private set; }

        public DescriptionModel(string body, int fontSize)
        {
            Body = body;
            FontSize = fontSize;
        }
    }
}

