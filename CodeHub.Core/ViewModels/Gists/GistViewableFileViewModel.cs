using System.Text;
using System.Windows.Input;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using ReactiveUI.Legacy;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewableFileViewModel : LoadableViewModel
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

        public ICommand GoToFileSourceCommand { get; private set; }

        public GistViewableFileViewModel(IApplicationService applicationService)
        {
            GoToFileSourceCommand = new ReactiveAsyncCommand();

            LoadCommand.RegisterAsyncTask(async t =>
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

                var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                    System.IO.Path.GetTempFileName() + ".html");
                System.IO.File.WriteAllText(path, data, Encoding.UTF8);
                FilePath = path;
            });
        }
    }
}
