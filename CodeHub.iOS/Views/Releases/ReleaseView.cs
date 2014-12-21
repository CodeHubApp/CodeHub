using System;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Releases;
using System.Reactive.Linq;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.Services;
using ReactiveUI;

namespace CodeHub.iOS.Views.Releases
{
    public class ReleaseView : ReactiveWebViewController<ReleaseViewModel>
    {
        public ReleaseView(INetworkActivityService networkActivityService)
            : base(networkActivityService)
        {
            Web.ScalesPageToFit = true;

            this.WhenViewModel(x => x.ShowMenuCommand)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x.ToBarButtonItem(UIBarButtonSystemItem.Action));

            this.WhenViewModel(x => x.ContentText).IsNotNull().Subscribe(contentText => 
            {
                var name = string.IsNullOrEmpty(ViewModel.ReleaseModel.Name) ? ViewModel.ReleaseModel.TagName : ViewModel.ReleaseModel.Name;
                var model = new ReleaseRazorViewModel { Body = contentText, Release = ViewModel.ReleaseModel, Name = name };
                LoadContent(new ReleaseRazorView { Model = model }.GenerateString());
            });
        }

        protected override bool ShouldStartLoad(MonoTouch.Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
                return base.ShouldStartLoad(request, navigationType);
            ViewModel.GoToLinkCommand.Execute(request.Url.AbsoluteString);
            return false;
        }
    }
}

