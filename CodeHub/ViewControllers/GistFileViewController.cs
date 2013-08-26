using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.ViewControllers
{
    public class GistFileViewController : FileSourceController
    {
        private string _url;
        public GistFileViewController(GistFileModel model)
        {
            _url = model.RawUrl;
            Title = model.Filename;
        }

        protected override void Request()
        {
            var data = Application.Client.Gists.GetFile(_url);
            LoadRawData(System.Security.SecurityElement.Escape(data));
        }
    }
}

