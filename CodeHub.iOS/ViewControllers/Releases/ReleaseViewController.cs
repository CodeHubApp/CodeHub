using System;
using UIKit;
using CodeHub.Core.ViewModels.Releases;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Views.Releases;
using Humanizer;

namespace CodeHub.iOS.ViewControllers.Releases
{
    public class ReleaseViewController : BaseWebViewController<ReleaseViewModel>
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
                .Subscribe(contentText => {
                    var release = ViewModel.ReleaseModel;
                    var name = string.IsNullOrEmpty(release.Name) ? release.TagName : release.Name;
                    var releaseTime = release.PublishedAt.HasValue ? release.PublishedAt.Value.UtcDateTime.Humanize() : release.CreatedAt.UtcDateTime.Humanize();
                    var model = new ReleaseRazorViewModel { Body = contentText, Author = release.Author.Login, Name = name, AuthorAvatarUrl = release.Author.AvatarUrl, ReleaseTime = releaseTime };
                    LoadContent(new ReleaseRazorView { Model = model }.GenerateString());
                });
        }

        protected override bool ShouldStartLoad(Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
                return base.ShouldStartLoad(request, navigationType);
            ViewModel.GoToLinkCommand.Execute(request.Url.AbsoluteString);
            return false;
        }
    }
}

