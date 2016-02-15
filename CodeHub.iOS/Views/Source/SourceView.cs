using System;
using UIKit;
using CodeHub.Core.ViewModels.Source;
using System.Threading.Tasks;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using CodeHub.iOS.WebViews;

namespace CodeHub.iOS.Views.Source
{
	public class SourceView : FileSourceView
    {
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            ViewModel.Bind(x => x.IsLoading).Subscribe(x => 
            {
                if (x) return;
				if (!string.IsNullOrEmpty(ViewModel.ContentPath))
				{
                    LoadSource(new Uri("file://" + ViewModel.ContentPath));
				}
				else if (!string.IsNullOrEmpty(ViewModel.FilePath))
				{
					LoadFile(ViewModel.FilePath);
				}
			});
		}

		protected override UIActionSheet CreateActionSheet(string title)
		{
            var editCommand = ((SourceViewModel)ViewModel).GoToEditCommand;
			var sheet = base.CreateActionSheet(title);
            var editButton = editCommand.CanExecute(null) ? sheet.AddButton("Edit") : -1;
            sheet.Dismissed += (sender, e) => BeginInvokeOnMainThread(() => {
                if (e.ButtonIndex == editButton)
                {
                    editCommand.Execute(null);
                    editCommand = null;
                }
            });
			return sheet;
		}

        async Task LoadSource(Uri fileUri)
        {
            var fontSize = (int)UIFont.PreferredSubheadline.PointSize;
            var content = System.IO.File.ReadAllText(fileUri.LocalPath, System.Text.Encoding.UTF8);

            if (ViewModel.IsMarkdown)
            {
                var markdownContent = await Mvx.Resolve<IApplicationService>().Client.Markdown.GetMarkdown(content);
                var model = new DescriptionModel(markdownContent, fontSize);
                var htmlContent = new MarkdownView { Model = model };
                LoadContent(htmlContent.GenerateString());
            }
            else
            {
                var model = new SourceBrowserModel(content, "idea", fontSize, fileUri.LocalPath);
                var contentView = new SyntaxHighlighterView { Model = model };
                LoadContent(contentView.GenerateString());
            }
        }
    }
}

