using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using System.Linq;
using System.IO;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Services;
using System.Reactive;
using Xamarin.Utilities.Factories;
using CodeHub.Core.ViewModels.Contents;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceViewModel : ContentViewModel, ILoadableViewModel
    {
        private static readonly string[] MarkdownExtensions = { ".markdown", ".mdown", ".mkdn", ".md", ".mkd", ".mdwn", ".mdtxt", ".mdtext", ".text" };

        public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public string Branch { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public string Path { get; set; }

        public bool ForceBinary { get; set; }

        public string GitUrl { get; set; }

        public string HtmlUrl { get; set; }

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

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public SourceViewModel(IApplicationService applicationService, IActionMenuFactory actionMenuFactory,
            IAccountsService accountsService, IFilesystemService filesystemService)
            : base(accountsService)
	    {
            var canEdit = this.WhenAnyValue(x => x.SourceItem, x => x.TrueBranch).Select(x => x.Item1 != null && x.Item2 && !x.Item1.IsBinary);
            GoToEditCommand = ReactiveCommand.Create(canEdit);
	        GoToEditCommand.Subscribe(_ =>
	        {
	            var vm = this.CreateViewModel<EditFileViewModel>();
                vm.Path = Path;
                vm.Branch = Branch;
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.SourceChanged.Subscribe(x => 
                {
                    GitUrl = x.Content.GitUrl;
                    LoadCommand.ExecuteIfCan();
                });
	            NavigateTo(vm);
            });

            this.WhenAnyValue(x => x.Name).Subscribe(x => Title = x ?? string.Empty);

            _isMarkdown = this.WhenAnyValue(x => x.Path).IsNotNull().Select(x => 
                MarkdownExtensions.Any(Path.EndsWith)).ToProperty(this, x => x.IsMarkdown);

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.SourceItem).Select(x => x != null),
                _ =>
            {
                var menu = actionMenuFactory.Create(Title);
                if (GoToEditCommand.CanExecute(null))
                    menu.AddButton("Edit", GoToEditCommand);
                menu.AddButton("Open With", OpenWithCommand);
                return menu.Show();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
	            string filepath;
                bool isBinary = false;

                using (var stream = filesystemService.CreateTempFile(out filepath, Name))
                {
                    if (MarkdownExtensions.Any(Path.EndsWith))
                    {
                        var renderedContent = await applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContentFileRendered(Path, Branch);
                        using (var stringWriter = new StreamWriter(stream))
                        {
                            await stringWriter.WriteAsync(renderedContent);
                        }
                    } 
                    else
                    {
                        if (string.IsNullOrEmpty(GitUrl))
                        {
                            var req = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContentFile(Path, Branch);
                            var data = (await applicationService.Client.ExecuteAsync(req)).Data;
                            GitUrl = data.GitUrl;
                        }

                        var mime = await applicationService.Client.DownloadRawResource2(GitUrl, stream) ?? string.Empty;
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