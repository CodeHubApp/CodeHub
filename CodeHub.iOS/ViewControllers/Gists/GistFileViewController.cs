using UIKit;
using System;
using CodeHub.WebViews;
using System.Threading.Tasks;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Gists;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class GistFileViewController : Views.Source.FileSourceView
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = ViewModel as GistFileViewModel;
            vm.Bind(x => x.ContentPath)
              .Where(x => x != null)
              .Subscribe(x => LoadSource(new Uri("file://" + x)).ToBackground());
        }

        async Task LoadSource(Uri fileUri)
        {
            var fontSize = (int)UIFont.PreferredSubheadline.PointSize;
            var content = System.IO.File.ReadAllText(fileUri.LocalPath, System.Text.Encoding.UTF8);

            if (ViewModel.IsMarkdown)
            {
                var markdownContent = await Mvx.Resolve<IApplicationService>().Client.Markdown.GetMarkdown(content);
                var model = new MarkdownModel(markdownContent, fontSize);
                var htmlContent = new MarkdownWebView { Model = model };
                LoadContent(htmlContent.GenerateString());
            }
            else
            {
                var zoom = UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone;
                var model = new SyntaxHighlighterModel(content, "idea", fontSize, zoom, fileUri.LocalPath);
                var contentView = new SyntaxHighlighterWebView { Model = model };
                LoadContent(contentView.GenerateString());
            }
        }
    }
}

