using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch;
using CodeHub.Controllers;
using CodeFramework.Views;
using CodeFramework.Controllers;

namespace CodeHub.ViewControllers
{
    public class WikiViewController : WebViewController
    {
        private static readonly string WikiCache = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wiki");

        private readonly string _user;
        private readonly string _slug;
        private readonly string _page;
        private readonly UIBarButtonItem _editButton;
        private ErrorView _errorView;
        private bool _isVisible;
        private bool _isLoaded;

        private void Load(string page, bool push = true, bool forceInvalidation = false)
        {
            this.DoWork(() => {
                if (_errorView != null)
                {
                    InvokeOnMainThread(delegate {
                        _errorView.RemoveFromSuperview();
                        _errorView = null;
                    });
                }

                var url = RequestAndSave(page, forceInvalidation);
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

		public WikiViewController(string user, string slug, string page = "Home")
        {
            _user = user;
            _slug = slug;
            if (page.StartsWith("/"))
                _page = page.Substring(1);
            else
                _page = page;
            Title = "Wiki".t();
            Web.ScalesPageToFit = true;
            Web.DataDetectorTypes = UIDataDetectorType.None;
            Web.ShouldStartLoad = ShouldStartLoad;

            _editButton = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.EditButton, HandleEditButton)) { Enabled = false };
        }

        private void HandleEditButton()
        {
//            try
//            {
//                var page = CurrentWikiPage(Web.Request);
//                var wiki = Application.Client.Users[_user].Repositories[_slug].Wikis[page].GetInfo();
//
//
//                var composer = new Composer { Title = "Edit ".t() + Title, Text = wiki.Data, ActionButtonText = "Save".t() };
//                composer.NewComment(this, () => {
//                    var text = composer.Text;
//
//                    composer.DoWork(() => {
//                        Application.Client.Users[_user].Repositories[_slug].Wikis[page].Update(text, Uri.UnescapeDataString("/" + page));
//                        
//                        InvokeOnMainThread(() => {
//                            composer.CloseComposer();
//                            Refresh();
//                        });
//                    }, ex =>
//                    {
//                        Utilities.ShowAlert("Unable to update page!", ex.Message);
//                        composer.EnableSendButton = true;
//                    });
//                });
//            }
//            catch (Exception e)
//            {
//                Utilities.ShowAlert("Error", e.Message);
//            }
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
                Load(_page);
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

        private bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navType)
        {
            try {
                if (navType == UIWebViewNavigationType.LinkClicked) 
                {
                    if (request.Url.ToString().Substring(0, 7).Equals("wiki://"))
                    {
                        Load(request.Url.ToString().Substring(7));
                        return false;
                    }
                }
            }
            catch
            {
            }

            return true;
        }

        protected override void Refresh()
        {
            var page = CurrentWikiPage(Web.Request);
            if (page != null)
            {
                if (RefreshButton != null)
                    RefreshButton.Enabled = false;
                this.DoWork(() => { 
                    RequestAndSave(page, true);
                    InvokeOnMainThread(base.Refresh);
                }, ex => {
                    if (_isVisible)
                        Utilities.ShowAlert("Unable to Find Wiki Page", ex.Message);
                    if (RefreshButton != null)
                        RefreshButton.Enabled = true;
                });
            }
            else
                base.Refresh();
        }

        protected override void OnLoadStarted(object sender, EventArgs e)
        {
            base.OnLoadStarted(sender, e);
            _editButton.Enabled = false;
        }

        protected override void OnLoadFinished(object sender, EventArgs e)
        {
            base.OnLoadFinished(sender, e);
            Title = Uri.UnescapeDataString(Web.EvaluateJavascript("document.title"));

            if (CurrentWikiPage(Web.Request) != null)
            {
                _editButton.Enabled = true;
                if (NavigationItem.RightBarButtonItem == null)
                    NavigationItem.SetRightBarButtonItem(_editButton, true);
            }
            else
                NavigationItem.SetRightBarButtonItem(null, true);
        }

        private string CurrentWikiPage(NSUrlRequest request)
        {
            var url = request.Url.AbsoluteString;
            if (!url.StartsWith("file://"))
                return null;
            var s = url.LastIndexOf('/');
            if (s < 0)
                return null;
            if (url.Length < s + 1)
                return null;

            url = url.Substring(s + 1);
            return url.Substring(0, url.LastIndexOf(".html")); //Get rid of ".html"
        }

        private string RequestAndSave(string page, bool forceInvalidation)
        {
//            var wiki = Application.Client.Users[_user].Repositories[_slug].Wikis[page];
//            var d = wiki.GetInfo(forceInvalidation);
//            var dataHtml = String.Empty;
//
//            if (d.Markup.Equals("markdown"))
//            {
//                var markdown = new MarkdownSharp.Markdown();
//                dataHtml = markdown.Transform(d.Data);
//            }
//            else if (d.Markup.Equals("creole"))
//            {
//                var w = new Wiki.CreoleParser();
//                w.OnLink += HandleOnLink;
//                dataHtml = w.ToHTML(d.Data);
//            }
//            else if (d.Markup.Equals("textile"))
//            {
//                dataHtml = Markup.Textile.Transform(d.Data);
//            }
//            else if (d.Markup.Equals("rest"))
//            {
//                //Need a parser for reStructuredText!!!
//                dataHtml = d.Data;
//            }
//            else
//            {
//                dataHtml = d.Data;
//            }
//
//            //Generate the markup
//            var markup = new System.Text.StringBuilder();
//            markup.Append("<html><head>");
//            markup.Append("<meta name=\"viewport\" content=\"width=device-width; initial-scale=1.0; maximum-scale=1.0; user-scalable=0\"/>");
//            markup.Append("<title>");
//            markup.Append(page);
//            markup.Append("</title></head><body>");
//            markup.Append(dataHtml);
//            markup.Append("</body></html>");
//
//            var url = System.IO.Path.Combine(WikiCache, Uri.UnescapeDataString(page) + ".html");
//            using (var file = System.IO.File.Create(url))
//            {
//                using (var writer = new System.IO.StreamWriter(file, System.Text.Encoding.UTF8))
//                {
//                    writer.Write(markup.ToString());
//                }
//            }
//
//            return url;
            return string.Empty;
        }

        void HandleOnLink (object sender, Wiki.LinkEventArgs e)
        {
            if (e.Href.Contains("://"))
            {
                e.Target = Wiki.LinkEventArgs.TargetEnum.External;
            }
            else
            {
                e.Target = Wiki.LinkEventArgs.TargetEnum.Internal;
                e.Href = "wiki://" + e.Href;
            }
        }
    }
}

