using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeHub.Core;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.Services;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.Views.Source;
using CodeHub.WebViews;
using Foundation;
using ReactiveUI;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class FileSourceViewController : BaseWebViewController
    {
        private static readonly string[] MarkdownExtensions = { ".markdown", ".mdown", ".mkdn", ".md", ".mkd", ".mdwn", ".mdtxt", ".mdtext", ".text" };

        private readonly IApplicationService _applicationService;
        private readonly IAlertDialogService _alertDialogService;
        private readonly string _username;
        private readonly string _repository;
        private readonly string _sha;
        private readonly ShaType _shaType;
        private readonly string _path;
        private readonly bool _forceBinary;
        private readonly bool _isMarkdown;
        private IDisposable _messageBus;

        private Octokit.RepositoryContent _content;
        public Octokit.RepositoryContent Content
        {
            get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }

        private string _contentSavePath;
        public string ContentSavePath
        {
            get { return _contentSavePath; }
            set { this.RaiseAndSetIfChanged(ref _contentSavePath, value); }
        }

        private bool _canEdit;
        public bool CanEdit
        {
            get { return _canEdit; }
            set { this.RaiseAndSetIfChanged(ref _canEdit, value); }
        }

        public FileSourceViewController(
            string username,
            string repository,
            string path,
            string sha,
            ShaType shaType,
            bool forceBinary = false,
            IApplicationService applicationService = null,
            IAlertDialogService alertDialogService = null,
            IMessageService messageService = null)
            : base(false)
        {
            _username = username;
            _repository = repository;
            _path = path;
            _sha = sha;
            _shaType = shaType;
            _forceBinary = forceBinary;
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _alertDialogService = alertDialogService ?? Locator.Current.GetService<IAlertDialogService>();
            messageService = messageService ?? Locator.Current.GetService<IMessageService>();

            Title = System.IO.Path.GetFileName(path);

            var extension = System.IO.Path.GetExtension(path);
            _isMarkdown = MarkdownExtensions.Contains(extension);

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
                d(this.WhenAnyValue(x => x.Content)
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

            _messageBus = messageService.Listen<Core.Messages.SourceEditMessage>(_ =>
            {
                Content = null;
                loadCommand.ExecuteNow();
            });
        }

        private UserError HandleLoadError(Exception error)
        {
            LoadContent("");
            return new UserError("Unable to load selected file.", error);
        }

        private async Task Load(CancellationToken cancelToken)
        {
            CanEdit = false;

            if (Content == null)
            {
                var encodedShaRef = System.Web.HttpUtility.UrlEncode(_sha);

                var items = await _applicationService
                    .GitHubClient.Repository.Content
                    .GetAllContentsByRef(_username, _repository, _path, encodedShaRef);
                Content = items.First();
            }

            var repo = await _applicationService
                .GitHubClient.Repository.Get(_username, _repository);

            var fileName = System.IO.Path.GetFileName(Content.Name);
            ContentSavePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);

            var mime = string.Empty;
            using (var stream = new System.IO.FileStream(ContentSavePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                mime = await _applicationService.Client.DownloadRawResource2(Content.GitUrl, stream) ?? string.Empty;

            var isBinary = _forceBinary || !mime.Contains("charset");
            if (isBinary)
            {
                LoadFile(ContentSavePath);
            }
            else
            {
                CanEdit = repo.Permissions.Push && _shaType == ShaType.Branch; 
                await LoadSource(new Uri("file://" + ContentSavePath), _isMarkdown);
            }
        }

        private void EditSource()
        {
            var vc = new EditSourceView();
            vc.ViewModel.Init(new EditSourceViewModel.NavObject { Path = _path, Branch = _sha, Username = _username, Repository = _repository });
            vc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            vc.NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => DismissViewController(true, null);
            PresentViewController(new ThemedNavigationController(vc), true, null);
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
            var url = Content?.HtmlUrl;
            if (url == null)
                return;

            AlertDialogService.Share(
                Title,
                url: Content?.HtmlUrl,
                barButtonItem: barButtonItem);
        }

        private void ShowInBrowser()
        {
            var url = Content?.HtmlUrl;
            if (url == null)
                return;

            var viewController = new WebBrowserViewController(url);
            PresentViewController(viewController, true, null);
        }

        private void CreateActionSheet(UIBarButtonItem barButtonItem)
        {
            var sheet = new UIActionSheet();
            sheet.Dismissed += (sender, e) => sheet.Dispose();

            var editButton = CanEdit ? sheet.AddButton("Edit") : -1;
            var openButton = ContentSavePath != null ? sheet.AddButton("Open In") : -1;
            var shareButton = Content?.HtmlUrl != null ? sheet.AddButton("Share") : -1;
            var showButton = Content?.HtmlUrl != null ? sheet.AddButton("Show in GitHub") : -1;
            var cancelButton = sheet.AddButton("Cancel");

            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (sender, e) => BeginInvokeOnMainThread(() => {
                try
                {
                    if (e.ButtonIndex == editButton)
                        EditSource();
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
