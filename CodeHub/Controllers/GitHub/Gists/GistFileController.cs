using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.GitHub.Controllers.Gists
{
    public class GistFileController : FileSourceController
    {
        private string _url;
        public GistFileController(GistFileModel model)
        {
            _url = model.RawUrl;
            Title = model.Filename;
        }

        protected override void Request()
        {
            var data = Application.Client.API.GetGistFile(_url);
            LoadRawData(System.Security.SecurityElement.Escape(data));
        }
    }
}

