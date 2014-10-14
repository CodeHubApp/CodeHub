using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.Views.Source;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.WebViews;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.Gists
{
	public class GistFileView : FileSourceView<GistFileViewModel>
    {
        private UIActionSheet _actionSheet;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            this.WhenViewModel(x => x.GistFile).Select(x => x != null).Subscribe(isValid =>
            {
                if (isValid)
                {
                    NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => CreateActionSheet());
                    NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.SourceItem).Select(x => x != null));
                }
                else
                {
                    NavigationItem.RightBarButtonItem = null;
                }
            });

            ViewModel.OpenWithCommand.Subscribe(x =>
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

        protected void CreateActionSheet()
        {
            _actionSheet = new UIActionSheet(Title);
            var openButton = ViewModel.OpenWithCommand.CanExecute(null) ? _actionSheet.AddButton("Open With") : -1;
            var themeButton = !ViewModel.IsMarkdown ? _actionSheet.AddButton("Change Theme") : -1;
            _actionSheet.CancelButtonIndex = _actionSheet.AddButton("Cancel");
            _actionSheet.Clicked += (sender, e) =>
            {
                if (e.ButtonIndex == themeButton)
                    ShowThemePicker();
                if (e.ButtonIndex == openButton)
                    ViewModel.OpenWithCommand.ExecuteIfCan();
                _actionSheet = null;
            };
            _actionSheet.ShowInView(View);
        }
    }
}

