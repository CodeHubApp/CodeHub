using System;

namespace CodeHub.iOS.WebViews
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

