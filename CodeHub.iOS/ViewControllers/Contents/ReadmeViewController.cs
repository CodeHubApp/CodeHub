using System;
using UIKit;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.ViewModels.Contents;
using CodeHub.WebViews;

namespace CodeHub.iOS.ViewControllers.Contents
{
    public class ReadmeViewController : BaseWebViewController<ReadmeViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Web.ScalesPageToFit = true;

            this.WhenAnyValue(x => x.ViewModel.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.ContentText)
                .IsNotNull()
                .Select(x => new DescriptionModel(x, (int)UIFont.PreferredSubheadline.PointSize))
                .Select(x => new MarkdownView { Model = x }.GenerateString())
                .Subscribe(LoadContent);
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

