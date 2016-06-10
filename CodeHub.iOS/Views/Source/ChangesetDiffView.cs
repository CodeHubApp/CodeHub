using System;
using UIKit;
using Foundation;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Utilities;
using CodeHub.Core.Services;
using WebKit;
using CodeHub.iOS.Services;
using Splat;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetDiffView : FileSourceView
    {
        private readonly IJsonSerializationService _serializationService = MvvmCross.Platform.Mvx.Resolve<IJsonSerializationService>();
        private bool _domLoaded = false;
        private List<string> _toBeExecuted = new List<string>();

        public new ChangesetDiffViewModel ViewModel
        {
            get { return (ChangesetDiffViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            ViewModel.Bind(x => x.FilePath).Subscribe(x =>
            {
                var data = System.IO.File.ReadAllText(x, System.Text.Encoding.UTF8);
                var patch = JavaScriptStringEncode(data);
                ExecuteJavascript("var a = \"" + patch + "\"; patch(a);");
            });

            ViewModel.BindCollection(x => x.Comments).Subscribe(e =>
            {
                //Convert it to something light weight
                var slimComments = ViewModel.Comments.Items.Where(x => string.Equals(x.Path, ViewModel.Filename)).Select(x => new { 
                    Id = x.Id, User = x.User.Login, Avatar = x.User.AvatarUrl, LineTo = x.Position, LineFrom = x.Position,
                    Content = x.Body, Date = x.UpdatedAt
                }).ToList();

                var c = _serializationService.Serialize(slimComments);
                ExecuteJavascript("var a = " + c + "; setComments(a);");
            });
        }

        private bool _isLoaded;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (!_isLoaded)
            {
                try 
                {
                    var path = System.IO.Path.Combine (NSBundle.MainBundle.BundlePath, "Diff", "diffindex.html");
                    var uri = Uri.EscapeUriString ("file://" + path) + "#" + Environment.TickCount;
                    Web.LoadRequest (new NSUrlRequest (new NSUrl (uri)));
                    _isLoaded = true;
                } 
                catch (Exception e)
                {
                    this.Log().ErrorException("Unable to load ChangesetDiffView", e);
                }
            }
        }

        private class JavascriptCommentModel
        {
            public int PatchLine { get; set; }
            public int FileLine { get; set; }
        }

        protected override bool ShouldStartLoad(WKWebView webView, WKNavigationAction navigationAction)
        {
            var url = navigationAction.Request.Url;
            if(url.Scheme.Equals("app")) {
                var func = url.Host;

                if (func.Equals("ready"))
                {
                    _domLoaded = true;
                    foreach (var e in _toBeExecuted)
                        Web.EvaluateJavaScript(e, null);
                }
                else if(func.Equals("comment")) 
                {
                    var commentModel = _serializationService.Deserialize<JavascriptCommentModel>(UrlDecode(url.Fragment));
                    PromptForComment(commentModel);
                }

                return false;
            }

            return base.ShouldStartLoad(webView, navigationAction);
        }

        private void ExecuteJavascript(string data)
        {
            if (_domLoaded)
                InvokeOnMainThread(() => Web.EvaluateJavaScript(data, null));
            else
                _toBeExecuted.Add(data);
        }

        private void PromptForComment(JavascriptCommentModel model)
        {
            var title = "Line " + model.FileLine;
            var sheet = new UIActionSheet(title);
            var addButton = sheet.AddButton("Add Comment");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (sender, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == addButton)
                        ShowCommentComposer(model.PatchLine);
                });

                sheet.Dispose();
            };

            sheet.ShowInView(this.View);
        }

        private void ShowCommentComposer(int line)
        {
            var composer = new MarkdownComposerViewController();
            composer.NewComment(this, async (text) => {
                try
                {
                    await composer.DoWorkAsync("Commenting...", () => ViewModel.PostComment(text, line));
                    composer.CloseComposer();
                }
                catch (Exception e)
                {
                    AlertDialogService.ShowAlert("Unable to Comment", e.Message);
                    composer.EnableSendButton = true;
                }
            });
        }
    }
}

