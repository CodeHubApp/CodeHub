using GitHubSharp.Models;
using CodeFramework.Controllers;
using MonoTouch.UIKit;
using CodeFramework.Views;

namespace CodeHub.ViewControllers
{
    public class GistFileViewController : FileSourceController
    {
        GistFileModel _model;
        private string _url;
        private string _content;

        public GistFileViewController(GistFileModel model, string content = null)
        {
            _url = model.RawUrl;
            _model = model;
            Title = model.Filename;
            _content = content;
        }

        protected override void Request()
        {
            if (_content == null)
                _content = Application.Client.Gists.GetFile(_url);
            var ext = System.IO.Path.GetExtension(_model.Filename).TrimStart('.');
            LoadRawData(System.Security.SecurityElement.Escape(_content), ext);
        }
    }

    public class GistViewableFileController : WebViewController
    {
        GistFileModel _model;
        string _rawContent;
        bool _loaded = false;

        public GistViewableFileController(GistFileModel model)
            : base(true)
        {
            _model = model;
            Title = model.Filename;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.ViewButton, () => {
                NavigationController.PushViewController(new GistFileViewController(model, _rawContent), true);
            }));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //Do the request
            if (_loaded == false)
            {
                this.DoWork(() => {
                    string data = _rawContent;
                    if (data == null)
                        data = _rawContent = Application.Client.Gists.GetFile(_model.RawUrl);
                    if (_model.Language.Equals("Markdown"))
                        data = Application.Client.Markdown.GetMarkdown(data);

                    var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName() + ".html");
                    System.IO.File.WriteAllText(path, data, System.Text.Encoding.UTF8);
                    LoadFile(path);
                    _loaded = true;
                });
            }
        }

        protected override void OnLoadError(object sender, UIWebErrorArgs e)
        {
            base.OnLoadError(sender, e);

            //Can't load this!
            MonoTouch.Utilities.ShowAlert("Error", "Unable to display this type of file.");
        }

        protected void LoadRawData(string data)
        {
            InvokeOnMainThread(delegate {
                Web.LoadHtmlString(data, null);
            });
        }
    }
}

