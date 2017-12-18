using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core;
using CodeHub.Core.Services;
using CodeHub.iOS.Services;
using CodeHub.WebViews;
using Foundation;
using ReactiveUI;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class GistFileViewController : BaseWebViewController
    {
        private readonly IApplicationService _applicationService;
        private readonly IAlertDialogService _alertDialogService;
        private readonly string _gistId;
        private readonly string _filename;

        private Octokit.Gist _gist;
        public Octokit.Gist Gist
        {
            get { return _gist; }
            set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        private string _contentSavePath;
        public string ContentSavePath
        {
            get { return _contentSavePath; }
            set { this.RaiseAndSetIfChanged(ref _contentSavePath, value); }
        }

        public GistFileViewController(
            string gistId,
            string filename,
            Octokit.Gist gist = null,
            IApplicationService applicationService = null,
            IAlertDialogService alertDialogService = null,
            IMessageService messageService = null)
            : base(false)
        {
            _gistId = gistId;
            _filename = filename;
            _gist = gist;
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _alertDialogService = alertDialogService ?? Locator.Current.GetService<IAlertDialogService>();
            messageService = messageService ?? Locator.Current.GetService<IMessageService>();

            Title = System.IO.Path.GetFileName(filename);

            var actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action) { Enabled = false };
            NavigationItem.RightBarButtonItem = actionButton;

            var loadCommand = ReactiveCommand.CreateFromTask(Load);

            loadCommand
                .ThrownExceptions
                .Select(HandleLoadError)
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            this.OnActivation(d =>
            {
                d(this.WhenAnyValue(x => x.Gist)
                  .Select(x => x != null)
                  .Subscribe(x => actionButton.Enabled = x));

                d(actionButton
                  .GetClickedObservable()
                  .Subscribe(CreateActionSheet));

                d(loadCommand
                  .IsExecuting
                  .Subscribe(x => actionButton.Enabled = !x));
            });

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(loadCommand);
        }

        private UserError HandleLoadError(Exception error)
        {
            LoadContent("");
            return new UserError("Unable to load selected file.", error);
        }

        private async Task Load()
        {
            if (Gist == null)
                Gist = await _applicationService.GitHubClient.Gist.Get(_gistId);

            if (!Gist.Files.ContainsKey(_filename))
                throw new Exception($"This gist does not have a file named {_filename}.");

            var file = Gist.Files[_filename];
            var isMarkdown = string.Equals(file.Language, "Markdown");

            var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _filename);
            System.IO.File.WriteAllText(filepath, file.Content, System.Text.Encoding.UTF8);
            ContentSavePath = filepath;
            await LoadSource(new Uri("file://" + filepath), isMarkdown);
        }

        async Task LoadSource(Uri fileUri, bool isMarkdown)
        {
            var fontSize = (int)UIFont.PreferredSubheadline.PointSize;
            var content = await Task.Run(() => System.IO.File.ReadAllText(fileUri.LocalPath, System.Text.Encoding.UTF8));
            await LoadSource(content, fileUri.LocalPath, isMarkdown);
        }

        async Task LoadSource(string content, string filename, bool isMarkdown)
        {
            var fontSize = (int)UIFont.PreferredSubheadline.PointSize;

            if (isMarkdown)
            {
                var markdownContent = await _applicationService.Client.Markdown.GetMarkdown(content);
                var model = new MarkdownModel(markdownContent, fontSize);
                var htmlContent = new MarkdownWebView { Model = model };
                LoadContent(htmlContent.GenerateString());
            }
            else
            {
                var zoom = UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone;
                var theme = _applicationService.Account.CodeEditTheme;
                var model = new SyntaxHighlighterModel(content, theme, fontSize, zoom, file: filename);
                var contentView = new SyntaxHighlighterWebView { Model = model };
                LoadContent(contentView.GenerateString());
            }
        }

        private void PresentOpenIn(UIBarButtonItem barButtonItem)
        {
            if (ContentSavePath == null)
                return;

            var ctrl = new UIDocumentInteractionController();
            ctrl.Url = NSUrl.FromFilename(ContentSavePath);
            ctrl.PresentOpenInMenu(barButtonItem, true);
        }

        private void Share(UIBarButtonItem barButtonItem)
        {
            var url = Gist?.HtmlUrl;
            if (url == null)
                return;

            AlertDialogService.Share(
                Title,
                url: Gist?.HtmlUrl,
                barButtonItem: barButtonItem);
        }

        private void ShowInBrowser()
        {
            var url = Gist?.HtmlUrl;
            if (url == null)
                return;

            var viewController = new WebBrowserViewController(url);
            PresentViewController(viewController, true, null);
        }

        private void CreateActionSheet(UIBarButtonItem barButtonItem)
        {
            var sheet = new UIActionSheet();
            sheet.Dismissed += (sender, e) => sheet.Dispose();

            var openButton = ContentSavePath != null ? sheet.AddButton("Open In") : -1;
            var shareButton = Gist?.HtmlUrl != null ? sheet.AddButton("Share") : -1;
            var showButton = Gist?.HtmlUrl != null ? sheet.AddButton("Show in GitHub") : -1;
            var cancelButton = sheet.AddButton("Cancel");

            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (sender, e) => BeginInvokeOnMainThread(() => {
                try
                {
                    if (e.ButtonIndex == openButton)
                        PresentOpenIn(barButtonItem);
                    else if (e.ButtonIndex == shareButton)
                        Share(barButtonItem);
                    else if (e.ButtonIndex == showButton)
                        ShowInBrowser();
                }
                catch
                {
                }
            });

            sheet.ShowFrom(barButtonItem, true);
        }
    }
}


