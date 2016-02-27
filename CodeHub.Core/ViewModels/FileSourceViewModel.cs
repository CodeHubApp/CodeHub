using System.Linq;
using System.Windows.Input;
using CodeHub.Core.ViewModels;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels
{
    public abstract class FileSourceViewModel : LoadableViewModel
    {
        private static readonly string[] BinaryMIMEs = new string[] 
        { 
            "image/", "video/", "audio/", "model/", "application/pdf", "application/zip", "application/gzip"
        };

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            protected set { this.RaiseAndSetIfChanged(ref _filePath, value); }
        }

        public bool IsMarkdown { get; protected set; }

        private string _contentPath;
        public string ContentPath
        {
            get { return _contentPath; }
            protected set { this.RaiseAndSetIfChanged(ref _contentPath, value); }
        }

        public string HtmlUrl
        {
            get;
            protected set;
        }

        public ICommand GoToHtmlUrlCommand
        {
            get { return new MvxCommand(() => ShowViewModel<WebBrowserViewModel>(new WebBrowserViewModel.NavObject { Url = HtmlUrl }), () => !string.IsNullOrEmpty(HtmlUrl)); }
        }

        protected static string CreatePlainContentFile(string data, string filename)
        {
            var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename);
            System.IO.File.WriteAllText(filepath, data, System.Text.Encoding.UTF8);
            return filepath;
        }

        protected static bool IsBinary(string mime)
        {
            var lowerMime = mime.ToLower();
            return BinaryMIMEs.Any(lowerMime.StartsWith);
        }
    }
}

