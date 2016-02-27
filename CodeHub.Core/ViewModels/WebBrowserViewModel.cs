namespace CodeHub.Core.ViewModels
{
    public class WebBrowserViewModel : BaseViewModel
    {
        public string Url { get; private set; }

        public void Init(NavObject navObject)
        {
            Url = navObject.Url;
        }

        public class NavObject
        {
            public string Url { get; set; }
        }
    }
}

