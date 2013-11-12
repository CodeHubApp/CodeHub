using GitHubSharp.Models;
using CodeFramework.Views;
using CodeFramework.ViewControllers;

namespace CodeHub.ViewControllers
{
    public class GistFileViewController : RawContentViewController
    {
        GistFileModel _model;
        private string _content;

        public GistFileViewController(GistFileModel model, string gistUrl, string content = null)
            : base(model.RawUrl, gistUrl)
        {
            _model = model;
            Title = model.Filename;
            _content = content;
        }

        protected override void Request()
        {
            if (_content == null)
            {
                base.Request();
            }
            else
            {
                var filePath = CreateFile(_model.Filename);
                System.IO.File.WriteAllText(filePath, _content, System.Text.Encoding.UTF8);
                _downloadResult = new DownloadResult { File = filePath, IsBinary = false };
                var ext = System.IO.Path.GetExtension(_model.Filename).TrimStart('.');
                LoadRawData(System.Security.SecurityElement.Escape(_content), ext);
            }
        }
    }
}

