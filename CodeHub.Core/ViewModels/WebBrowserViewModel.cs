using System;

namespace CodeHub.Core.ViewModels
{
    public class WebBrowserViewModel : BaseViewModel
    {
        public Uri Uri { get; private set; }

        public WebBrowserViewModel Init(string url)
        {
            if (url == null)
                return this;

            if (!url.StartsWith("http", StringComparison.Ordinal) && !url.Contains("://"))
                url = "http://" + url;

            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                Uri = uri;
            return this;
        }

        public WebBrowserViewModel Init(Uri uri)
        {
            Uri = uri;
            return this;
        }
    }
}

