using System;
using CodeFramework.iOS.Views;
using CodeHub.iOS;
using CodeHub.iOS.Views.Source;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using GitHubSharp;
using CodeFramework.Views;

namespace CodeHub.ViewControllers
{
	public class RawContentViewController : FileSourceViewController
    {
        private readonly string _rawUrl;
        private readonly string _githubUrl;
        protected DownloadResult _downloadResult;

        public RawContentViewController(string rawUrl, string githubUrl)
        {
            _rawUrl = rawUrl;
            _githubUrl = githubUrl;
            Title = rawUrl.Substring(rawUrl.LastIndexOf('/') + 1);
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.GearButton, ShowExtraMenu));
        }

        private void ShowExtraMenu()
        {
            var sheet = MonoTouch.Utilities.GetSheet(Title);

            var openButton = _downloadResult != null ? sheet.AddButton("Open In".t()) : -1;
            var shareButton = sheet.AddButton("Share".t());
            var showButton = _githubUrl != null ? sheet.AddButton("Show in GitHub".t()) : -1;
            var cancelButton = sheet.AddButton("Cancel".t());
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Clicked += (s, e) => {
                if (e.ButtonIndex == openButton)
                {
                    var ctrl = new UIDocumentInteractionController();
                    ctrl.Url = NSUrl.FromFilename(_downloadResult.File);
                    ctrl.PresentOpenInMenu(NavigationItem.RightBarButtonItem, true);
                }
                else if (e.ButtonIndex == shareButton)
                {
                    var item = UIActivity.FromObject (_githubUrl);
                    var activityItems = new NSObject[] { item };
                    UIActivity[] applicationActivities = null;
                    var activityController = new UIActivityViewController (activityItems, applicationActivities);
                    PresentViewController (activityController, true, null);
                }
                else if (e.ButtonIndex == showButton)
                {
                    try { UIApplication.SharedApplication.OpenUrl(new NSUrl(_githubUrl)); } catch { }
                }
            };

            sheet.ShowInView(this.View);
        }


        protected override void Request()
        {
            try 
            {
                var result = _downloadResult = DownloadFile(_rawUrl);
                var ext = System.IO.Path.GetExtension(_rawUrl).TrimStart('.');
                if (!result.IsBinary)
                    LoadRawData(System.Security.SecurityElement.Escape(System.IO.File.ReadAllText(result.File, System.Text.Encoding.UTF8)), ext);
                else
                    LoadFile(result.File);
            }
            catch (InternalServerException ex)
            {
                MonoTouch.Utilities.ShowAlert("Error", ex.Message);
            }
        }
    }

    public class SourceInfoViewController : RawContentViewController
    {
        public SourceInfoViewController(string rawHtmlUrl, string path)
            : base (ReplaceFirst(rawHtmlUrl, "/blob/", "/raw/"), rawHtmlUrl)
        {
            //Create the filename
            var fileName = System.IO.Path.GetFileName(path);
            if (fileName == null)
                fileName = path.Substring(path.LastIndexOf('/') + 1);

            //Create the temp file path
            Title = fileName;
        }

        private static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
                return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}

