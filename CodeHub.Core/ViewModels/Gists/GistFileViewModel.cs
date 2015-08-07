using System;
using CodeHub.Core.Services;
using ReactiveUI;
using CodeHub.Core.ViewModels.Source;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.Factories;
using Octokit;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistFileViewModel : ContentViewModel, ILoadableViewModel
    {
        public const string MarkdownLanguage = "Markdown";

        private string _id;
	    public string Id
	    {
	        get { return _id; }
	        set { this.RaiseAndSetIfChanged(ref _id, value); }
	    }

        private string _filename;
	    public string Filename
	    {
	        get { return _filename; }
	        set { this.RaiseAndSetIfChanged(ref _filename, value); }
	    }

        private GistFile _gistFile;
        public GistFile GistFile
	    {
	        get { return _gistFile; }
	        set { this.RaiseAndSetIfChanged(ref _gistFile, value); }
	    }

        private readonly ObservableAsPropertyHelper<bool> _isMarkdown;
        public override bool IsMarkdown
        {
            get { return _isMarkdown.Value; }
        }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public GistFileViewModel(ISessionService sessionService, ISessionService applicationService, 
            IFilesystemService filesystemService, IActionMenuFactory actionMenuService)
            : base(sessionService)
	    {
	        this.WhenAnyValue(x => x.Filename)
                .Select(x => x == null ? "Gist" : x.Substring(x.LastIndexOf('/') + 1))
                .Subscribe(x => Title = x);
                
            _isMarkdown = this.WhenAnyValue(x => x.GistFile)
                .IsNotNull()
                .Select(x => string.Equals(x.Language, MarkdownLanguage, StringComparison.OrdinalIgnoreCase))
                .ToProperty(this, x => x.IsMarkdown);

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.SourceItem).Select(x => x != null),
                sender => {
                    var menu = actionMenuService.Create();
                    menu.AddButton("Open With", OpenWithCommand);
                    return menu.Show(sender);
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                if (GistFile == null)
			    {
                    var data = await applicationService.GitHubClient.Gist.Get(_id);
                    GistFile = data.Files[_filename];
			    }

			    //Check to make sure...
                if (GistFile == null || GistFile.Content == null)
			    {
				    throw new Exception("Unable to retreive gist!");
			    }

                var content = GistFile.Content;
                if (MarkdownLanguage.Equals(GistFile.Language, StringComparison.OrdinalIgnoreCase))
                    content = await applicationService.GitHubClient.Miscellaneous.RenderRawMarkdown(content);

                var gistFileName = System.IO.Path.GetFileName(GistFile.Filename).Trim();
                string filePath;
                
                using (var stream = filesystemService.CreateTempFile(out filePath, gistFileName))
                {
                    using (var fs = new System.IO.StreamWriter(stream))
                    {
                        fs.Write(content);
                    }
                }

                var fileUri = new Uri(filePath);
                SourceItem = new FileSourceItemViewModel(fileUri, false);
            });
	    }
    }
}

