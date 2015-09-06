using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using GitHubSharp.Models;
using CodeFramework.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewableFileViewModel : LoadableViewModel
    {
        private GistFileModel _gistFile;
        private string _filePath;

        public GistFileModel GistFile
        {
            get { return _gistFile; }
            set { _gistFile = value; RaisePropertyChanged(() => GistFile); }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; RaisePropertyChanged(() => FilePath); }
        }

        public ICommand GoToFileSourceCommand
        {
            get { return new MvxCommand(() => { });}
        }

        public void Init(NavObject navObject)
        {
            GistFile = navObject.GistFile;
        }

		protected override async Task Load(bool forceCacheInvalidation)
        {
            string data;
            using (var ms = new System.IO.MemoryStream())
            {
				await this.GetApplication().Client.DownloadRawResource(GistFile.RawUrl, ms);
                ms.Position = 0;
                var sr = new System.IO.StreamReader(ms);
                data = sr.ReadToEnd();
            }
            if (GistFile.Language.Equals("Markdown"))
				data = await this.GetApplication().Client.Markdown.GetMarkdown(data);

            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName() + ".html");
            System.IO.File.WriteAllText(path, data, Encoding.UTF8);
            FilePath = path;
        }

        public class NavObject
        {
			public string Filename { get; set; }
			public string RawUrl { get; set; }
            public GistFileModel GistFile { get; set; }
        }
    }
}
