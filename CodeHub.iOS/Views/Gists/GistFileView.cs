using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.Views.Source;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.WebViews;
using MonoTouch.Foundation;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.iOS.Views.Gists
{
	public class GistFileView : FileSourceView<GistFileViewModel>
    {
        public GistFileView(INetworkActivityService networkActivityService)
            : base(networkActivityService)
        {
            this.WhenViewModel(x => x.GistFile).IsNotNull().Subscribe(x =>
                NavigationItem.RightBarButtonItem = ViewModel.ShowMenuCommand.ToBarButtonItem(UIBarButtonSystemItem.Action));

            this.WhenViewModel(x => x.OpenWithCommand)
                .Switch()
                .Subscribe(_ =>
                {
                    UIDocumentInteractionController ctrl = UIDocumentInteractionController.FromUrl(new NSUrl(ViewModel.SourceItem.FileUri.AbsoluteUri));
                    ctrl.Delegate = new UIDocumentInteractionControllerDelegate();
                    ctrl.PresentOpenInMenu(this.View.Frame, this.View, true);
                });
        }

        protected override void LoadSource(Uri fileUri)
        {
            if (ViewModel.GistFile == null)
                return;

            if (ViewModel.IsMarkdown)
            {
                var content = System.IO.File.ReadAllText(fileUri.LocalPath, System.Text.Encoding.UTF8);
                var htmlContent = new MarkdownView { Model = content };
                LoadContent(htmlContent.GenerateString());
            }
            else
                base.LoadSource(fileUri);
        }

        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
                return base.ShouldStartLoad(request, navigationType);
            ViewModel.GoToUrlCommand.ExecuteIfCan(request.Url.AbsoluteString);
            return false;
        }
    }
}

