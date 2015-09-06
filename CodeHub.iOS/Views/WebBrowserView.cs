using System;

namespace CodeFramework.iOS.Views
{
	public class WebBrowserView : WebView
    {
		public override void ViewDidLoad()
		{
			Title = "Web";

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

