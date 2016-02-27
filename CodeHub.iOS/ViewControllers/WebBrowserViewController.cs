using System;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.ViewControllers
{
    public class WebBrowserViewController : WebView
    {
        public WebBrowserViewController()
            : base(true, true)
        {
            Title = "Web";
        }
        
        public WebBrowserViewController(string url)
            : this()
        {
            var vm = new WebBrowserViewModel();
            vm.Init(new WebBrowserViewModel.NavObject { Url = url });
            ViewModel = vm;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                var vm = (WebBrowserViewModel)ViewModel;
                if (!string.IsNullOrEmpty(vm.Url))
                    Web.LoadRequest(new Foundation.NSUrlRequest(new Foundation.NSUrl(vm.Url)));
            }
            catch (Exception e)
            {
                AlertDialogService.ShowAlert("Unable to process request!", e.Message);
            }
        }
    }
}

