using System;
using CodeHub.Core.ViewModels.Repositories;
using MonoTouch.UIKit;
using System.Reactive.Linq;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.Services;

namespace CodeHub.iOS.Views.Repositories
{
    public class ReadmeView : ReactiveWebViewController<ReadmeViewModel>
    {
        public ReadmeView(INetworkActivityService networkActivityService)
            : base(networkActivityService)
        {
            Web.ScalesPageToFit = true;

            this.WhenViewModel(x => x.ContentText).IsNotNull().Subscribe(x =>
                LoadContent(new ReadmeRazorView { Model = x }.GenerateString()));

            this.WhenViewModel(x => x.ShowMenuCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = x.ToBarButtonItem(UIBarButtonSystemItem.Action));
        }

		protected override bool ShouldStartLoad(MonoTouch.Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
		    if (request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
		        return base.ShouldStartLoad(request, navigationType);
            ViewModel.GoToLinkCommand.ExecuteIfCan(request.Url.AbsoluteString);
		    return false;
		}
    }
}

