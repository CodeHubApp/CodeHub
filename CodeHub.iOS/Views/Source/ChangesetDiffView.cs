using System;
using UIKit;
using Foundation;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Utilities;
using WebKit;
using CodeHub.iOS.Services;
using Splat;
using Newtonsoft.Json;
using CodeHub.WebViews;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetDiffView : FileSourceView
    {
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

                var diffModel = new DiffModel(
                    data.Split('\n'),
                    Enumerable.Empty<DiffCommentModel>(),
                    (int)UIFont.PreferredSubheadline.PointSize);
                
                var diffView = new DiffWebView { Model = diffModel };
                LoadContent(diffView.GenerateString());
            });

            ViewModel.BindCollection(x => x.Comments).Subscribe(e =>
            {
                //Convert it to something light weight
                var slimComments = ViewModel.Comments.Items.Where(x => string.Equals(x.Path, ViewModel.Filename)).Select(x => new { 
                    Id = x.Id, User = x.User.Login, Avatar = x.User.AvatarUrl, LineTo = x.Position, LineFrom = x.Position,
                    Content = x.Body, Date = x.UpdatedAt
                }).ToList();

                var c = JsonConvert.SerializeObject(slimComments);
                ExecuteJavascript("var a = " + c + "; setComments(a);");
            });
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

                if(func.Equals("comment")) 
                {
                    var commentModel = JsonConvert.DeserializeObject<JavascriptCommentModel>(UrlDecode(url.Fragment));
                    PromptForComment(commentModel);
                }

                return false;
            }

            return base.ShouldStartLoad(webView, navigationAction);
        }

        private void ExecuteJavascript(string data)
        {

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
            composer.PresentAsModal(this, async () => {
                UIApplication.SharedApplication.BeginIgnoringInteractionEvents();

                try
                {
                    await composer.DoWorkAsync("Commenting...", () => ViewModel.PostComment(composer.Text, line));
                    this.DismissViewController(true, null);
                }
                catch (Exception e)
                {
                    AlertDialogService.ShowAlert("Unable to Comment", e.Message);
                }

                UIApplication.SharedApplication.EndIgnoringInteractionEvents();

            });
        }
    }
}

