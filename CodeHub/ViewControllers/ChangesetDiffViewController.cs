using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.Controllers;

namespace CodeHub.ViewControllers
{
    public class ChangesetDiffViewController : FileSourceViewController
    {
        private readonly CommitModel.CommitFileModel _commit;
        public List<CommentModel> Comments;
        private readonly string _user;
        private readonly string _slug;
        private readonly string _branch;

        public ChangesetDiffViewController(string user, string slug, string branch, CommitModel.CommitFileModel commit)
        {
            _commit = commit;
            _user = user;
            _slug = slug;
            _branch = branch;

            //Create the filename
            var fileName = System.IO.Path.GetFileName(commit.Filename);
            if (fileName == null)
                fileName = commit.Filename.Substring(commit.Filename.LastIndexOf('/') + 1);
            Title = fileName;
        }

        protected override void Request()
        {
            // Must be a binary file
            if (_commit.Patch == null)
            {
                string mime;
                var file = DownloadFile(_commit.RawUrl, out mime);
                LoadFile(file);
                return;
            }
            else
            {
                LoadDiffData();
            }
        }

        private class JavascriptCommentModel
        {
            public int Line { get; set; }
        }

        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            var url = request.Url;
            if(url.Scheme.Equals("app")) {
                var func = url.Host;
                if(func.Equals("comment")) {
                    PromptForComment(new RestSharp.Deserializers.JsonDeserializer().Deserialize<JavascriptCommentModel>(new RestSharp.RestResponse { Content = Decode(url.Fragment) }).Line);
                    return false;
                }
            }

            return base.ShouldStartLoad(request, navigationType);
        }

        private void PromptForComment(int line)
        {
            string title = string.Empty;
            title = "Line ".t() + line;

            var sheet = MonoTouch.Utilities.GetSheet(title);
            var addButton = sheet.AddButton("Add Comment".t());
            var cancelButton = sheet.AddButton("Cancel".t());
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Clicked += (sender, e) => {
                if (e.ButtonIndex == addButton)
                    ShowCommentComposer(line);
            };

            sheet.ShowInView(this.View);
        }

        private void ShowCommentComposer(int line)
        {
            var composer = new Composer();
            composer.NewComment(this, () => {
                var text = composer.Text;
                composer.DoWork(() => {
                    var c = Application.Client.Users[_user].Repositories[_slug].Commits[_branch].Comments.Create(text, _commit.Filename, line).Data;

                    //This will inheriently add it to the controller's comments which we're referencing
                    if (Comments != null)
                        Comments.Add(c);

                    var a = new List<CommentModel>();
                    a.Add(c);
                    AddComments(a);

                    InvokeOnMainThread(() => composer.CloseComposer());
                }, ex => {
                    MonoTouch.Utilities.ShowAlert("Unable to Comment".t(), ex.Message);
                    composer.EnableSendButton = true;
                });
            });
        }

        protected override void DOMReady()
        {
            //Binary file, nothing to do..
            if (_commit.Patch == null)
                return;

            var patch = JavaScriptStringEncode(_commit.Patch);
            InvokeOnMainThread(() => Web.EvaluateJavascript("var a = \"" + patch + "\"; patch(a);"));
            AddComments(Comments);
        }

        private void AddComments(List<CommentModel> comments)
        {
            //Convert it to something light weight
            var slimComments = comments.Where(x => string.Equals(x.Path, _commit.Filename)).Select(x => new { 
                Id = x.Id, User = x.User.Login, Avatar = x.User.AvatarUrl, LineTo = x.Position, LineFrom = x.Position,
                Content = x.Body, Date = x.UpdatedAt
            }).ToList();

            var c = new RestSharp.Serializers.JsonSerializer().Serialize(slimComments);
            BeginInvokeOnMainThread(() => Web.EvaluateJavascript("var a = " + c + "; addComments(a);"));
        }
    }
}

