using System.Text;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewableFileViewModel : BaseViewModel, ILoadableViewModel
    {
        private GistFileModel _gistFile;
        private string _filePath;

        public GistFileModel GistFile
        {
            get { return _gistFile; }
            set { this.RaiseAndSetIfChanged(ref _gistFile, value); }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { this.RaiseAndSetIfChanged(ref _filePath, value); }
        }

        public IReactiveCommand<object> GoToFileSourceCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public GistViewableFileViewModel(IApplicationService applicationService, IFilesystemService filesystemService)
        {
            GoToFileSourceCommand = ReactiveCommand.Create();

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                string data;
                using (var ms = new System.IO.MemoryStream())
                {
                    await applicationService.Client.DownloadRawResource(GistFile.RawUrl, ms);
                    ms.Position = 0;
                    var sr = new System.IO.StreamReader(ms);
                    data = sr.ReadToEnd();
                }
                if (GistFile.Language.Equals("Markdown"))
                    data = await applicationService.Client.Markdown.GetMarkdown(data);

                string path;
                using (var stream = filesystemService.CreateTempFile(out path, "gist.html"))
                {
                    using (var fs = new System.IO.StreamWriter(stream))
                    {
                        fs.Write(data);
                    }
                }

                FilePath = path;
            });
        }
    }
}
