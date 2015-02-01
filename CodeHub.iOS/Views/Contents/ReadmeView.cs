using System;
using UIKit;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Views.Contents;
using CodeHub.Core.ViewModels.Contents;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Views.Contents
{
    public class ReadmeView : BaseWebView<ReadmeViewModel>
    {
        public ReadmeView()
        {
            Web.ScalesPageToFit = true;

            this.WhenAnyValue(x => x.ViewModel.ContentText).IsNotNull().Subscribe(x =>
                LoadContent(new ReadmeRazorView { Model = x }.GenerateString()));

            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }

		protected override bool ShouldStartLoad(Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
		    if (request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
		        return base.ShouldStartLoad(request, navigationType);
            ViewModel.GoToLinkCommand.ExecuteIfCan(request.Url.AbsoluteString);
		    return false;
		}
    }
}

