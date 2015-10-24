using System;

namespace CodeFramework.iOS.Views
{
	public class WebBrowserView : WebView
    {
        public WebBrowserView()
            : base(true, true)
        {
            Title = "Web";
        }

		public override void ViewDidLoad()
		{

			base.ViewDidLoad();
			var vm = (CodeFramework.Core.ViewModels.WebBrowserViewModel)ViewModel;
            try
            {
    			if (!string.IsNullOrEmpty(vm.Url))
    				Web.LoadRequest(new Foundation.NSUrlRequest(new Foundation.NSUrl(vm.Url)));
            }
            catch (Exception e)
            {
                MonoTouch.Utilities.ShowAlert("Unable to process request!", e.Message);
            }
		}
    }
}

