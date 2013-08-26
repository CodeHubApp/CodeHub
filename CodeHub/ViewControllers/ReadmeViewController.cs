using System;
using CodeHub.Controllers;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch;
using CodeFramework.Controllers;
using CodeFramework.Views;

namespace CodeHub.GitHub.Controllers.Readme
{
    public class ReadmeViewController : WebViewController
    {
        private static readonly string WikiCache = Utilities.BaseDir + "/tmp/WikiCache/";
        private readonly string _user;
        private readonly string _slug;
        private ErrorView _errorView;
        private bool _isVisible;
        private bool _isLoaded;

        public ReadmeViewController(string user, string slug)
        {
            _user = user;
            _slug = slug;
            Title = "Readme";
            Web.ScalesPageToFit = true;
            Web.DataDetectorTypes = UIDataDetectorType.None;
        }

        private string RequestAndSave(bool forceInvalidation)
        {
            var wiki = Application.Client.Users[_user].Repositories[_slug].GetReadme().Data;
            var d = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(wiki.Content));
            var data = Application.Client.Markdown.GetMarkdown(d);

            //Generate the markup
            var markup = new System.Text.StringBuilder();
            markup.Append("<html><head>");
            markup.Append("<meta name=\"viewport\" content=\"width=device-width; initial-scale=1.0; maximum-scale=1.0; user-scalable=0\"/>");
            markup.Append("<title>Readme");
            markup.Append("</title></head><body>");
            markup.Append(data);
            markup.Append("</body></html>");

            var url = WikiCache  + "readme.html";
            using (var file = System.IO.File.Create(url))
            {
                using (var writer = new System.IO.StreamWriter(file, System.Text.Encoding.UTF8))
                {
                    writer.Write(markup.ToString());
                }
            }

            return url;
        }
        
        private void Load(bool forceInvalidation = false)
        {
            this.DoWork(() => {
                if (_errorView != null)
                {
                    InvokeOnMainThread(delegate {
                        _errorView.RemoveFromSuperview();
                        _errorView = null;
                    });
                }
                
                var url = RequestAndSave(forceInvalidation);
                var escapedUrl = Uri.EscapeUriString("file://" + url);
                var request = NSUrlRequest.FromUrl(new NSUrl(escapedUrl));
                NSUrlCache.SharedCache.RemoveCachedResponse(request);
                InvokeOnMainThread(() => Web.LoadRequest(request));
            },
            ex => {
                if (_isVisible)
                    Utilities.ShowAlert("Unable to Find Wiki Page", ex.Message);
            });

        }
        
        public override void ViewDidDisappear(bool animated)
        {
            _isVisible = false;
            base.ViewDidDisappear(animated);
        }
        
        public override void ViewDidAppear(bool animated)
        {
            _isVisible = true;
            base.ViewDidAppear(animated);
            
            //Load the page
            if (!_isLoaded)
                Load();
            _isLoaded = true;
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            //Delete the cache directory just incase it already exists..
            if (System.IO.Directory.Exists(WikiCache))
                System.IO.Directory.Delete(WikiCache, true);
            System.IO.Directory.CreateDirectory(WikiCache);
        }

        protected override void Refresh()
        {
            if (Web.Request.Url.AbsoluteString.StartsWith("file://"))
            {
                if (RefreshButton != null)
                    RefreshButton.Enabled = false;
                this.DoWork(() => { 
                    RequestAndSave(true);
                    InvokeOnMainThread(base.Refresh);
                }, ex => {
                    if (_isVisible)
                        Utilities.ShowAlert("Unable to refresh readme!", ex.Message);
                    if (RefreshButton != null)
                        RefreshButton.Enabled = true;
                });
            }
            else
                base.Refresh();
        }
    }
}

