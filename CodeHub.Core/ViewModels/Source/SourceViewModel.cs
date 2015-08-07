using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using System.Linq;
using System.IO;
using System.Reactive;
using CodeHub.Core.ViewModels.Contents;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceViewModel : ContentViewModel, ILoadableViewModel
    {
        private static readonly string[] MarkdownExtensions = { ".markdown", ".mdown", ".mkdn", ".md", ".mkd", ".mdwn", ".mdtxt", ".mdtext", ".text" };

        public string Branch { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public string Path { get; set; }

        public bool ForceBinary { get; set; }

        public string GitUrl { get; set; }

        public string HtmlUrl { get; set; }

        private bool? _pushAccess;
        public bool? PushAccess
        {
            get { return _pushAccess; }
            set { this.RaiseAndSetIfChanged(ref _pushAccess, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        private readonly ObservableAsPropertyHelper<bool> _isMarkdown;
        public override bool IsMarkdown
        {
            get { return _isMarkdown.Value; }
        }

        private bool _trueBranch;
        public bool TrueBranch
        {
            get { return _trueBranch; }
            set { this.RaiseAndSetIfChanged(ref _trueBranch, value); }
        }

        public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public IReactiveCommand<object> OpenInGitHubCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public SourceViewModel(ISessionService sessionService, IActionMenuFactory actionMenuFactory,
            IFilesystemService filesystemService)
            : base(sessionService)
	    {
            var canEdit = this.WhenAnyValue(x => x.SourceItem, x => x.TrueBranch, x => x.PushAccess)
                .Select(x => x.Item1 != null && x.Item2 && !x.Item1.IsBinary && x.Item3.HasValue && x.Item3.Value);
            GoToEditCommand = ReactiveCommand.Create(canEdit);
	        GoToEditCommand.Subscribe(_ => {
	            var vm = this.CreateViewModel<EditFileViewModel>();
                vm.Init(RepositoryOwner, RepositoryName, Path, null, Branch);
                vm.SaveCommand.Subscribe(x => {
                    GitUrl = x.Content.GitUrl.AbsoluteUri;
                    LoadCommand.ExecuteIfCan();
                });
	            NavigateTo(vm);
            });

            this.WhenAnyValue(x => x.Name).Subscribe(x => Title = x ?? string.Empty);

            _isMarkdown = this.WhenAnyValue(x => x.Path).IsNotNull().Select(x => 
                MarkdownExtensions.Any(Path.EndsWith)).ToProperty(this, x => x.IsMarkdown);

            OpenInGitHubCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.HtmlUrl).Select(x => !string.IsNullOrEmpty(x)));
            OpenInGitHubCommand
                .Select(_ => this.CreateViewModel<WebBrowserViewModel>())
                .Select(x => x.Init(this.HtmlUrl))
                .Subscribe(NavigateTo);

            var canShowMenu = this.WhenAnyValue(x => x.SourceItem).Select(x => x != null);
            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(canShowMenu, sender => {
                var menu = actionMenuFactory.Create();
                if (GoToEditCommand.CanExecute(null))
                    menu.AddButton("Edit", GoToEditCommand);
                menu.AddButton("Open With", OpenWithCommand);
                if (OpenInGitHubCommand.CanExecute(null))
                    menu.AddButton("Open in GitHub", OpenInGitHubCommand);
                return menu.Show(sender);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
	            string filepath;
                bool isBinary = false;

                if (!PushAccess.HasValue)
                {
                    sessionService.GitHubClient.Repository.Get(RepositoryOwner, RepositoryName)
                        .ToBackground(x => PushAccess = x.Permissions.Push);
                }

                using (var stream = filesystemService.CreateTempFile(out filepath, Name))
                {
                    if (MarkdownExtensions.Any(Path.EndsWith))
                    {
                        var renderedContent = await sessionService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContentFileRendered(Path, Branch);
                        using (var stringWriter = new StreamWriter(stream))
                        {
                            await stringWriter.WriteAsync(renderedContent);
                        }
                    } 
                    else
                    {
                        if (string.IsNullOrEmpty(GitUrl) || string.IsNullOrEmpty(HtmlUrl))
                        {
                            var req = sessionService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContentFile(Path, Branch);
                            var data = (await sessionService.Client.ExecuteAsync(req)).Data;
                            GitUrl = data.GitUrl;
                            HtmlUrl = data.HtmlUrl;
                        }

                        var mime = await sessionService.Client.DownloadRawResource2(GitUrl, stream) ?? string.Empty;
                        isBinary = !(mime ?? string.Empty).Contains("charset");
                    }
                }

                // We can force a binary representation if it was passed during init. In which case we don't care to figure out via the mime.
                var fileUri = new Uri(filepath);
                SourceItem = new FileSourceItemViewModel(fileUri, ForceBinary || isBinary);
	        });
	    }
    }
}