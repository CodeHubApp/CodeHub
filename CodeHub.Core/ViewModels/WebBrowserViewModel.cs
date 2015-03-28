namespace CodeHub.Core.ViewModels
{
    public class WebBrowserViewModel : BaseViewModel
    {
        public string Url { get; private set; }

        public WebBrowserViewModel Init(string url)
        {
            Url = url;
            return this;
        }
    }
}

